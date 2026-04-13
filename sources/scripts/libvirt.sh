#!/usr/bin/env bash
set -euo pipefail

export LIBVIRT_DEFAULT_URI="qemu:///system"

BRIDGE="br0" 
STORAGE_POOL="default"

VMS=(
  "phoenix-100 02:00:00:00:01:00"
  "phoenix-101 02:00:00:00:01:01"
  "phoenix-102 02:00:00:00:01:02"
)

for entry in "${VMS[@]}"; do
  read -r NAME MAC <<< "$entry"

  echo "=== Processing VM $NAME ($MAC) ==="

  if virsh dominfo "$NAME" >/dev/null 2>&1; then
      echo "  -> Found existing VM. Purging..."
      virsh destroy "$NAME" >/dev/null 2>&1 || true
      
      virsh undefine "$NAME" --remove-all-storage --nvram --snapshots-metadata --managed-save >/dev/null 2>&1 || \
      virsh undefine "$NAME" --remove-all-storage >/dev/null 2>&1 || \
      virsh undefine "$NAME"
  fi

  echo "  -> Starting virt-install for $NAME..."
  virt-install \
    --name "$NAME" \
    --memory 4096 \
    --vcpus 2 \
    --cpu host-passthrough \
    --disk pool="$STORAGE_POOL",size=12,bus=virtio \
    --network bridge="$BRIDGE",mac="$MAC",model=virtio \
    --boot uefi,network,hd \
    --os-variant fedora-unknown \
    --serial pty \
    --console pty,target_type=serial \
    --graphics none \
    --noautoconsole

  echo "  -> $NAME is provisioning in the background."
  echo "-----------------------------------------------"
done

echo "=== All VMs deployed ==="
echo "To monitor the PXE boot process, open a new terminal and run:"
for entry in "${VMS[@]}"; do
  read -r NAME MAC <<< "$entry"
  echo "  sudo virsh console $NAME"
done
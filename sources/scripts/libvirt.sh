#!/usr/bin/env bash
set -euo pipefail

export LIBVIRT_DEFAULT_URI="qemu:///system"

BRIDGE="br0" 
STORAGE_POOL="default"

VMS=(
  "phoenix-100 02:00:00:00:01:00"
  "phoenix-101 02:00:00:00:01:01"
  "phoenix-102 02:00:00:00:01:02"
#  "phoenix-103 02:00:00:00:01:03"
#  "phoenix-104 02:00:00:00:01:04"
#  "phoenix-105 02:00:00:00:01:05"
#  "phoenix-106 02:00:00:00:01:06"
#  "phoenix-107 02:00:00:00:01:07"
#  "phoenix-108 02:00:00:00:01:08"
#  "phoenix-109 02:00:00:00:01:09"
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

#  --network type=direct,source=eno1,source_mode=bridge,mac="$MAC",model=e1000e \

  echo "  -> Starting virt-install for $NAME..."
    virt-install \
      --name "$NAME" \
      --memory 4096 \
      --vcpus 2 \
      --cpu host-passthrough \
      --controller type=scsi,model=virtio-scsi \
      --disk pool="$STORAGE_POOL",size=16,bus=scsi \
      --network network=default,mac="$MAC",model=e1000e \
      --boot uefi,hd,network \
      --os-variant fedora-unknown \
      --graphics spice,listen=0.0.0.0 \
      --video qxl \
      --channel spicevmc \
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

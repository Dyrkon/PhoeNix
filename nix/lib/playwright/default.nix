{
  pkgs,
  lib,
}: let
  runtimeLibs = with pkgs; [
    alsa-lib
    at-spi2-atk
    at-spi2-core
    atk
    cairo
    cups
    dbus
    expat
    fontconfig
    freetype
    gdk-pixbuf
    glib
    gtk3
    libdrm
    libxkbcommon
    mesa
    nspr
    nss
    pango
    udev
    xorg.libX11
    xorg.libXcomposite
    xorg.libXcursor
    xorg.libXdamage
    xorg.libXext
    xorg.libXfixes
    xorg.libXi
    xorg.libXrandr
    xorg.libXrender
    xorg.libXtst
    xorg.libxcb
    xorg.libxshmfence
  ];

  # Shell code that sets CHROME_PATH and creates RUNSETTINGS.
  mkRunSettingsShell = ''
    export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
    export PLAYWRIGHT_BROWSERS_PATH="${pkgs.playwright-driver.browsers}"

    CHROME_PATH=""
    for p in "${pkgs.playwright-driver.browsers}"/chromium-*/chrome-linux/chrome; do
      if [ -x "$p" ]; then
        CHROME_PATH="$p"
        break
      fi
    done

    if [ -z "$CHROME_PATH" ]; then
      echo "ERROR: Could not find chromium under ${pkgs.playwright-driver.browsers}"
      ls -la "${pkgs.playwright-driver.browsers}" || true
      exit 1
    fi

    RUNSETTINGS="$(mktemp)"
    cat > "$RUNSETTINGS" <<EOF
    <?xml version="1.0" encoding="utf-8"?>
    <RunSettings>
    <Playwright>
        <BrowserName>chromium</BrowserName>
        <LaunchOptions>
        <ExecutablePath>$CHROME_PATH</ExecutablePath>
        </LaunchOptions>
    </Playwright>
    </RunSettings>
    EOF
  '';
in {
  inherit runtimeLibs mkRunSettingsShell;
}

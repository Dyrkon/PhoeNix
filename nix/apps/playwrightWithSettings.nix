{
  inputs,
  pkgs,
  ...
}: 
let
  chrome-version = "1091";
  chrome-path =
    if pkgs.stdenv.isLinux
    then "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-linux/chrome"
    else "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-mac/Chromium.app/Contents/MacOS/Chromium";

  # Create the RunSettings file
  runsettingsFile = pkgs.writeTextFile {
    name = "RunSettings.runsettings";
    text = ''
      <?xml version="1.0" encoding="utf-8"?>
      <RunSettings>
        <Playwright>
          <BrowserName>chromium</BrowserName>
          <LaunchOptions>
            <ExecutablePath>${chrome-path}</ExecutablePath>
          </LaunchOptions>
        </Playwright>
      </RunSettings>
    '';
  };
in
{
  type = "app";
  program = "${pkgs.writeShellScript "playwright" "${pkgs.dotnet-sdk_8}/bin/dotnet test --settings:${runsettingsFile}"}";
}

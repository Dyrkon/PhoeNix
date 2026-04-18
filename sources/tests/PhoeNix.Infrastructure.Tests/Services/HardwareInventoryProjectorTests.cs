using FluentAssertions;
using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Infrastructure.Services.HardwareManagement;

namespace PhoeNix.Infrastructure.Tests.Services;

public class HardwareInventoryProjectorTests
{
    private readonly HardwareInventoryProjector _sut = new();

    private static HardwareProbeResult MakeResult(string report)
        => new(report, DateTime.UtcNow);

    private const string MinimalReport = """
        {
          "hardware": {
            "cpu": [{"vendor_name":"GenuineIntel","model_name":"Intel Core i7","cores":4,"units":8}],
            "memory": [],
            "graphics_card": [],
            "disk": [],
            "monitor": [],
            "keyboard": [],
            "mouse": [],
            "sound": [],
            "network_interface": [],
            "usb": [],
            "bluetooth": []
          }
        }
        """;

    [Fact]
    public void Project_Should_Fail_When_Report_Is_Empty()
    {
        var result = _sut.Project(MakeResult(string.Empty));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HardwareInventoryProjectionEmptyReport");
    }

    [Fact]
    public void Project_Should_Fail_When_Report_Is_Whitespace()
    {
        var result = _sut.Project(MakeResult("   "));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HardwareInventoryProjectionEmptyReport");
    }

    [Fact]
    public void Project_Should_Fail_When_Hardware_Section_Missing()
    {
        var result = _sut.Project(MakeResult("""{"smbios": {}}"""));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HardwareInventoryProjectionMissingHardwareSection");
    }

    [Fact]
    public void Project_Should_Fail_For_Invalid_Json()
    {
        var result = _sut.Project(MakeResult("not-valid-json{{{"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("HardwareInventoryProjectionInvalidJson");
    }

    [Fact]
    public void Project_Should_Succeed_With_Minimal_Valid_Report()
    {
        var result = _sut.Project(MakeResult(MinimalReport));

        result.IsSuccess.Should().BeTrue();
        result.Value.Cpu.Should().NotBeNull();
        result.Value.Cpu!.Model.Should().Be("Intel Core i7");
        result.Value.Cpu.CoreCount.Should().Be(4);
        result.Value.Cpu.ThreadCount.Should().Be(8);
    }

    [Fact]
    public void Project_Should_Parse_Cpu_Vendor_And_Model()
    {
        var report = """
            {
              "hardware": {
                "cpu": [{"vendor_name":"AuthenticAMD","model_name":"AMD Ryzen 9","cores":16,"units":32}]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Cpu!.Vendor.Should().Be("AuthenticAMD");
        result.Value.Cpu.Model.Should().Be("AMD Ryzen 9");
        result.Value.Cpu.CoreCount.Should().Be(16);
        result.Value.Cpu.ThreadCount.Should().Be(32);
    }

    [Fact]
    public void Project_Should_Return_Null_Cpu_When_No_Cpu_Array()
    {
        var report = """{"hardware": {}}""";

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Cpu.Should().BeNull();
    }

    [Fact]
    public void Project_Should_Parse_Gpu_Profiles()
    {
        var report = """
            {
              "hardware": {
                "graphics_card": [
                  {
                    "model": "GeForce RTX 4090",
                    "vendor": {"name": "NVIDIA"},
                    "resources": [{"type": "memory", "range": 25769803776}]
                  }
                ]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Gpus.Should().ContainSingle();
        var gpu = result.Value.Gpus.Single();
        gpu.Model.Should().Be("GeForce RTX 4090");
        gpu.Vendor.Should().Be("NVIDIA");
        gpu.VramBytes.Should().Be(25769803776L);
    }

    [Fact]
    public void Project_Should_Parse_Disk_Profiles()
    {
        // Use lowercase "ssd" so TryInferRotational's case-sensitive Contains("ssd") fires
        var report = """
            {
              "hardware": {
                "disk": [
                  {
                    "model": "samsung ssd 870 evo",
                    "vendor": {"name": "Samsung"},
                    "bus_type": {"name": "SATA"},
                    "unix_device_names": ["/dev/disk/by-id/ata-Samsung_870_EVO", "/dev/sda"],
                    "resources": [{"type": "size", "unit": "sectors", "value_1": 976773168, "value_2": 512}]
                  }
                ]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Disks.Should().ContainSingle();
        var disk = result.Value.Disks.Single();
        disk.Model.Should().Be("samsung ssd 870 evo");
        disk.Vendor.Should().Be("Samsung");
        disk.BusType.Should().Be("SATA");
        disk.StableDevicePath.Should().Be("/dev/disk/by-id/ata-Samsung_870_EVO");
        disk.KernelDevicePath.Should().Be("/dev/sda");
        disk.SizeBytes.Should().Be(976773168L * 512L);
        disk.IsRotational.Should().BeFalse();
    }

    [Fact]
    public void Project_Should_Infer_NVMe_Disk_As_Non_Rotational()
    {
        var report = """
            {
              "hardware": {
                "disk": [
                  {
                    "bus_type": {"name": "NVMe"},
                    "unix_device_names": ["/dev/disk/by-id/nvme-Samsung_PM981", "/dev/nvme0n1"]
                  }
                ]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        var disk = result.Value.Disks.Single();
        disk.IsRotational.Should().BeFalse();
    }

    [Fact]
    public void Project_Should_Infer_USB_Disk_Rotational_As_Null()
    {
        var report = """
            {
              "hardware": {
                "disk": [
                  {
                    "bus_type": {"name": "usb"},
                    "unix_device_names": ["/dev/disk/by-path/usb-0:1", "/dev/sdb"]
                  }
                ]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        var disk = result.Value.Disks.Single();
        disk.IsRotational.Should().BeNull();
    }

    [Fact]
    public void Project_Should_Parse_Peripheral_Profiles()
    {
        var report = """
            {
              "hardware": {
                "keyboard": [{"model": "USB Keyboard"}],
                "mouse": [{"model": "USB Mouse"}],
                "monitor": [{"model": "Dell Monitor"}],
                "network_interface": [{"model": "Intel NIC"}]
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Peripherals.Should().HaveCount(4);
    }

    [Fact]
    public void Project_Should_Parse_Motherboard_From_Smbios()
    {
        var report = """
            {
              "hardware": {},
              "smbios": {
                "board": {
                  "manufacturer": "ASUS",
                  "product": "ROG STRIX Z790-E"
                }
              }
            }
            """;

        var result = _sut.Project(MakeResult(report));

        result.IsSuccess.Should().BeTrue();
        result.Value.Motherboard.Should().NotBeNull();
        result.Value.Motherboard!.Vendor.Should().Be("ASUS");
        result.Value.Motherboard.Model.Should().Be("ROG STRIX Z790-E");
    }

    [Fact]
    public void Project_Should_Return_Null_Motherboard_When_Smbios_Missing()
    {
        var result = _sut.Project(MakeResult(MinimalReport));

        result.IsSuccess.Should().BeTrue();
        result.Value.Motherboard.Should().BeNull();
    }
}

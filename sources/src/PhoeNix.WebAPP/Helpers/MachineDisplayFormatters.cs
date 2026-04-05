using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Helpers;

internal static class MachineDisplayFormatters
{
    public static string FormatString(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    public static string FormatInt(this int? value)
    {
        return value?.ToString() ?? "-";
    }

    public static string FormatBool(this bool? value)
    {
        return value switch
        {
            null => "-",
            true => "Yes",
            false => "No"
        };
    }

    public static string FormatBool(this bool value)
    {
        return value switch
        {
            true => "Yes",
            false => "No"
        };
    }

    public static string FormatDateTime(this DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    public static string FormatDateTime(this DateTime value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FormatMacAddress(this string value)
    {
        return value.ToMacFormat();
    }

    public static string FormatBytes(this long? value)
    {
        if (!value.HasValue)
            return "-";

        const double kilo = 1024d;
        const double mega = kilo * 1024d;
        const double giga = mega * 1024d;
        const double tera = giga * 1024d;

        var bytes = value.Value;

        if (bytes >= tera) return $"{bytes / tera:0.##} TB";
        if (bytes >= giga) return $"{bytes / giga:0.##} GB";
        if (bytes >= mega) return $"{bytes / mega:0.##} MB";
        if (bytes >= kilo) return $"{bytes / kilo:0.##} KB";

        return $"{bytes} B";
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domain.Addresses;

namespace WebApplication1.Models;

public sealed class SavedAddressRegion
{
    public int AddressId { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;
    public string DistrictCode { get; set; } = string.Empty;
    public string SubdistrictCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    [NotMapped]
    public AddressRegion Value => new(ProvinceCode, DistrictCode, SubdistrictCode, PostalCode);

    public void CopyFrom(SavedAddressRegion region)
    {
        ProvinceCode = region.ProvinceCode;
        DistrictCode = region.DistrictCode;
        SubdistrictCode = region.SubdistrictCode;
        PostalCode = region.PostalCode;
    }
}

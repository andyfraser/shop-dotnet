using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface IAddressService
{
    List<UserAddress> GetUserAddresses(int userId);
    UserAddress? GetAddress(int addressId, int userId);
    void AddAddress(UserAddress address);
    void UpdateAddress(UserAddress address);
    void DeleteAddress(int addressId, int userId);
    void SetDefaultAddress(int addressId, int userId);
}

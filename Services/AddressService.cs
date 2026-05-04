using Dapper;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class AddressService : IAddressService
{
    private readonly IDatabaseService _db;

    public AddressService(IDatabaseService db) => _db = db;

    public List<UserAddress> GetUserAddresses(int userId)
    {
        using var conn = _db.GetConnection();
        return conn.Query<UserAddress>(
            "SELECT * FROM user_addresses WHERE user_id = @userId ORDER BY is_default DESC, created_at DESC",
            new { userId }).ToList();
    }

    public UserAddress? GetAddress(int addressId, int userId)
    {
        using var conn = _db.GetConnection();
        return conn.QueryFirstOrDefault<UserAddress>(
            "SELECT * FROM user_addresses WHERE id = @addressId AND user_id = @userId",
            new { addressId, userId });
    }

    public void AddAddress(UserAddress address)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var trans = conn.BeginTransaction();
        try
        {
            if (address.IsDefault)
            {
                conn.Execute("UPDATE user_addresses SET is_default = 0 WHERE user_id = @UserId", new { address.UserId }, trans);
            }

            conn.Execute(
                "INSERT INTO user_addresses (user_id, label, full_name, address, city, postcode, country, is_default) VALUES (@UserId, @Label, @FullName, @Address, @City, @Postcode, @Country, @IsDefault)",
                address, trans);
            
            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public void UpdateAddress(UserAddress address)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var trans = conn.BeginTransaction();
        try
        {
            if (address.IsDefault)
            {
                conn.Execute("UPDATE user_addresses SET is_default = 0 WHERE user_id = @UserId", new { address.UserId }, trans);
            }

            conn.Execute(@"
                UPDATE user_addresses 
                SET label=@Label, full_name=@FullName, address=@Address, city=@City, 
                    postcode=@Postcode, country=@Country, is_default=@IsDefault 
                WHERE id=@Id AND user_id=@UserId",
                address, trans);
            
            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public void DeleteAddress(int addressId, int userId)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "DELETE FROM user_addresses WHERE id = @addressId AND user_id = @userId",
            new { addressId, userId });
    }

    public void SetDefaultAddress(int addressId, int userId)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var trans = conn.BeginTransaction();
        try
        {
            conn.Execute("UPDATE user_addresses SET is_default = 0 WHERE user_id = @userId", new { userId }, trans);
            conn.Execute("UPDATE user_addresses SET is_default = 1 WHERE id = @addressId AND user_id = @userId", new { addressId, userId }, trans);
            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }
}

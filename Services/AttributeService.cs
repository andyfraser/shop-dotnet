using Dapper;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class AttributeService : IAttributeService
{
    private readonly IDatabaseService _db;

    public AttributeService(IDatabaseService db) => _db = db;

    public List<AttributeValue> GetProductAttributes(int productId)
    {
        using var conn = _db.GetConnection();
        return conn.Query<AttributeValue>(
            @"SELECT av.*, a.name as AttributeName 
              FROM attribute_values av 
              JOIN attributes a ON av.attribute_id = a.id 
              JOIN product_attribute_values pav ON av.id = pav.attribute_value_id 
              WHERE pav.product_id = @productId",
            new { productId }).ToList();
    }

    public List<Models.Attribute> GetAllAttributes()
    {
        using var conn = _db.GetConnection();
        return conn.Query<Models.Attribute>("SELECT * FROM attributes ORDER BY name").ToList();
    }
}

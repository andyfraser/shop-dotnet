using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface IAttributeService
{
    List<AttributeValue> GetProductAttributes(int productId);
    List<Models.Attribute> GetAllAttributes();
}

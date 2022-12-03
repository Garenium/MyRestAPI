using Casestudy.DAL.DomainClasses;
using System.Text.Json;
namespace Casestudy.DAL
{
    public class DataUtility
    {
        private readonly AppDbContext _db;
        public DataUtility(AppDbContext context)
        {
            _db = context;
        }

        public async Task<bool> LoadProductInfoFromWebToDb(string stringJson)
        {
            bool brandsLoaded = false;
            bool productsLoaded = false;

            try
            {
                //an element that is typed as dynamic is assumed to support any operation
                dynamic? objectJson = JsonSerializer.Deserialize<Object>(stringJson);
                brandsLoaded = await LoadBrands(objectJson);
                productsLoaded = await LoadProducts(objectJson);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return productsLoaded && brandsLoaded;
        }

        private async Task<bool> LoadBrands(dynamic jsonObjectArray)
        {
            bool loadedBrands = false;
            try
            {
                //clear out the old rows
                _db.Brands?.RemoveRange(_db.Brands);
                await _db.SaveChangesAsync();

                List<String> allBrands = new();

                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {
                    if (element.TryGetProperty("BRAND", out JsonElement brandJson))
                    {
                        allBrands.Add(brandJson.GetString()!);
                    }
                }

                IEnumerable<String> brands = allBrands.Distinct<String>();

                foreach (string brandname in brands)
                {
                    Brand brand = new();
                    brand.Name = brandname;
                    await _db.Brands!.AddAsync(brand);
                    await _db.SaveChangesAsync();
                }

                loadedBrands = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loadedBrands;
        }


        public async Task<bool> LoadProducts(dynamic jsonObjectArray)
        {
            bool loadedProducts = false;

            try
            {
                List<Brand> brands = _db.Brands!.ToList();
                //clear out the old
                _db.Products?.RemoveRange(_db.Products);
                await _db.SaveChangesAsync();

                foreach (JsonElement element in jsonObjectArray.EnumerateArray())
                {
                    Product item = new();
                    item.ProductName = Convert.ToString(element.GetProperty("PRODUCT").GetString());
                    item.Description = Convert.ToString(element.GetProperty("DESCRIPTION").GetString());
                    item.CostPrice = Convert.ToDecimal(element.GetProperty("COSTPRICE").GetString());
                    item.MSRP = Convert.ToDecimal(element.GetProperty("MSRP").GetString());
                    item.QtyOnHand = Convert.ToInt32(element.GetProperty("QTYONHAND").GetString());
                    item.QtyOnBackOrder = Convert.ToInt32(element.GetProperty("QTYONBACK").GetString());
                    item.GraphicName = Convert.ToString(element.GetProperty("GRAPHIC").GetString());
                    item.Id = Convert.ToString(element.GetProperty("BRANDID").GetString());

                    string? pro = element.GetProperty("BRAND").GetString();

                    //add the fk here
                    foreach(Brand brand in brands)
                    {
                        if(brand.Name == pro)
                        {
                            item.Brand = brand;
                            break;
                        }
                    }
                    await _db.Products!.AddAsync(item);
                    await _db.SaveChangesAsync();
                }
                loadedProducts = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return loadedProducts;
        }
    }
}


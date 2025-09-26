using Microsoft.AspNetCore.Mvc;
using BackendOfReactProject.Models;
namespace BackendOfReactProject.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> _products = new List<Product>(){
        new Product{ Id=1, Name="Laptop" ,Price=45000},
        new Product{ Id=2, Name="Mobile" ,Price=2000 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(_products);
        }
        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id) {   
                    var product =_products.FirstOrDefault(x => x.Id == id);
            if (product == null) {return NotFound();}

             return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> Create(Product newProduct)
        {
            newProduct.Id =_products.Max(x => x.Id)+1;
            _products.Add(newProduct);
            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
        }
        //Put api/products/1
        [HttpPut("{id}")]
        public ActionResult Update(int id, Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;

            return NoContent(); // standard response for successful update
        }
        // DELETE: api/products/1
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            _products.Remove(product);
            return NoContent();
        }

    }
}

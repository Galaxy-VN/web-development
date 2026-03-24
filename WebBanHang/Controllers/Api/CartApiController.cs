using Microsoft.AspNetCore.Mvc;
using WebBanHang.Extensions;
using WebBanHang.Models;
using WebBanHang.Models.Dto;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Controllers.Api
{
    /// <summary>
    /// REST API for session-based shopping cart operations.
    /// </summary>
    [ApiController]
    [Route("api/cart")]
    public class CartApiController : ControllerBase
    {
        private const string CartSessionKey = "Cart";
        private readonly IProductRepository _productRepository;

        public CartApiController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Gets current cart from session.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        public ActionResult<CartResponse> GetCart()
        {
            var cart = GetCartFromSession();
            return Ok(ToCartResponse(cart));
        }

        /// <summary>
        /// Adds an item to cart. If item exists, increases quantity.
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartResponse>> AddItem([FromBody] CartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = request.Quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCartToSession(cart);
            return Ok(ToCartResponse(cart));
        }

        /// <summary>
        /// Updates quantity for an existing cart item.
        /// </summary>
        [HttpPut("items/{productId:int}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CartResponse> UpdateItem(int productId, [FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingItem == null)
            {
                return NotFound(new { message = "Item not found in cart." });
            }

            existingItem.Quantity = request.Quantity;
            SaveCartToSession(cart);

            return Ok(ToCartResponse(cart));
        }

        /// <summary>
        /// Removes one item from cart by product id.
        /// </summary>
        [HttpDelete("items/{productId:int}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        public ActionResult<CartResponse> RemoveItem(int productId)
        {
            var cart = GetCartFromSession();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCartToSession(cart);
            return Ok(ToCartResponse(cart));
        }

        /// <summary>
        /// Clears all items from cart.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return NoContent();
        }

        private List<CartItem> GetCartFromSession()
        {
            return HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.SetJson(CartSessionKey, cart);
        }

        private static CartResponse ToCartResponse(List<CartItem> cart)
        {
            var items = cart.Select(i => new CartItemResponse
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList();

            return new CartResponse
            {
                Items = items,
                TotalPrice = items.Sum(i => i.LineTotal),
                TotalQuantity = items.Sum(i => i.Quantity)
            };
        }
    }
}

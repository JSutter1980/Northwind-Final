using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using System.Linq;

public class CartController : Controller
{
  // this controller depends on the NorthwindRepository
  private DataContext _dataContext;
  public CartController(DataContext db) => _dataContext = db;
  public IActionResult Remove(int id) {
    // delete item from cart
    _dataContext.RemoveFromCart(id);
    // redirect back to Index
    return View("Index", _dataContext.CartItems.Include("Product").OrderBy(c => c.ProductId));
  }
  // make sure customer is authenticated
  [Authorize(Roles = "northwind-customer")]
  public IActionResult Index() => View(_dataContext.CartItems.Include("Product").Where(c => c.Customer.Email == User.Identity.Name).OrderBy(c => c.ProductId));

  [HttpPost, Authorize(Roles = "northwind-customer")]
  public IActionResult Checkout() {
    // Find all records based on the currently logged in user from cartitems table
    var items = _dataContext.CartItems.Where(c => c.Customer.Email == User.Identity.Name);
    int id = _dataContext.Customers.FirstOrDefault(c => c.Email == User.Identity.Name).CustomerId;
    DateTime od = DateTime.Now;
    DateTime rd = od.AddDays(7);
    // Add a row to the Orders table for the order
    Order order = new Order {CustomerId = id, OrderDate = od, RequiredDate = rd};
    int orderId = _dataContext.AddOrder(order);
    // Add rows to the OrderDetails table for each item in the cart
    List<OrderDetail> details = new List<OrderDetail>();
    foreach(CartItem item in items) {
      OrderDetail d = new OrderDetail();
      d.OrderId = orderId;
      d.ProductId = item.ProductId;
      d.Quantity = item.Quantity;
      // d.UnitPrice = _dataContext.Products.FirstOrDefault(p => p.ProductId == item.ProductId).UnitPrice;
      details.Add(d);
    }
    _dataContext.AddOrderDetails(details);
    // Remove cartitems from cartitems table
    _dataContext.RemoveCartItems(id);
    // redirect to shpping cart
    return RedirectToAction("Index");
  }
}
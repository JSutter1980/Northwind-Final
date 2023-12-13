using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CartController : Controller
{
  // this controller depends on the NorthwindRepository
  private DataContext _dataContext;
  public CartController(DataContext db) => _dataContext = db;
  public IActionResult Index() => View(_dataContext.CartItems.Include("Product").OrderBy(c => c.ProductId));


}
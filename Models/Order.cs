namespace hhpizza_backend.Models;

public class Order
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string CustomerPhone { get; set; }
    public string CustomerEmail { get; set; }
    public string OrderType { get; set; }
    public string PaymentType { get; set; }
    public decimal Tip { get; set; }
    public int? Review {  get; set; }
    public User User { get; set; }
    public ICollection<Item> Items { get; set; }

}

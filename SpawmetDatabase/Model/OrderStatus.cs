using System.ComponentModel.DataAnnotations;

namespace SpawmetDatabase.Model
{
    public enum OrderStatus : byte
    {
        [Display(Name="Nowe")]
        Start = 0,
    }
}

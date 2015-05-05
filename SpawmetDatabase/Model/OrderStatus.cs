using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpawmetDatabase.Model
{
    public enum OrderStatus : byte
    {
        //[Display(Name="Nowe")]
        //[Description("Nowe")]
        //[LocalizableDescription(@"Nowe", typeof(OrderStatusResource))]
        [LocalizedDescription("New", typeof(EnumResources))]
        New = 0,

        //[Description("W toku")]
        //[LocalizableDescription(@"W toku", typeof(OrderStatusResource))]
        //[Description("W toku")]
        [LocalizedDescription("InProgress", typeof(EnumResources))]
        InProgress = 1,

        //[LocalizableDescription(@"Zakończone", typeof(OrderStatusResource))]
        //[Description("Zakończone")]
        [LocalizedDescription("Done", typeof(EnumResources))]
        Done = 2,
    }
}

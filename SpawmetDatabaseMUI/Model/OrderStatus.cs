using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpawmetDatabase.Model
{
    public enum OrderStatus : byte
    {
        [LocalizedDescription("New", typeof(EnumResources))]
        New = 0,

        [LocalizedDescription("InProgress", typeof(EnumResources))]
        InProgress = 1,

        [LocalizedDescription("Done", typeof(EnumResources))]
        Done = 2,
    }
}

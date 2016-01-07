using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace SpawmetDatabase.Model
{
    public enum Origin : byte
    {
        [LocalizedDescription("Production", typeof(EnumResources))]
        Production = 0,

        [LocalizedDescription("Outside", typeof(EnumResources))]
        Outside = 1,
    }
}

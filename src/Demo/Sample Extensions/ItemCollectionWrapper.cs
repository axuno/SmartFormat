//
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.ComponentModel;

namespace Demo.Sample_Extensions;

public class InventoryItem {
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }
    public List<InventoryItem> Components { get; set; } = [];
    public override string ToString() => $"{Name} (Comp: {Components.Count})";
}

public class ItemCollectionWrapper
{
    // Use the ExpandableObjectConverter so the PropertyGrid
    // shows a '+' sign next to the property to expand it.
    [TypeConverter(typeof(ExpandableObjectConverter))]
    // Use the Category and Description attributes for better display in the grid
    [Category("Products")]
    [DisplayName("Items")]
    [Browsable(true)]
    public List<InventoryItem> Items { get; set; }

    public ItemCollectionWrapper(List<InventoryItem> items)
    {
        Items = items;
    }

    public override string ToString() => $"{Items.Count} items";

}

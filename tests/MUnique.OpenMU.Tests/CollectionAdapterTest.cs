// <copyright file="CollectionAdapterTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using System.Collections.Specialized;
using MUnique.OpenMU.Persistence;

/// <summary>
/// Tests the <see cref="CollectionAdapter{TClass, TEfCore}"/>.
/// </summary>
[TestFixture]
public class CollectionAdapterTest
{
    /// <summary>
    /// Tests that clearing a populated collection doesn't throw and still reports the removed
    /// items - a real seed initializer (<c>MerchantStores.ReplaceStore</c>) clears a merchant's
    /// pre-populated item list before re-adding a new catalog, which crashed the whole server
    /// startup because <see cref="NotifyCollectionChangedEventArgs"/> rejects a non-null item
    /// list for <see cref="NotifyCollectionChangedAction.Reset"/>.
    /// </summary>
    [Test]
    public void ClearOnPopulatedCollectionDoesNotThrowAndReportsOldItems()
    {
        var raw = new List<string> { "a", "b" };
        var adapter = new CollectionAdapter<string, string>(raw);
        NotifyCollectionChangedEventArgs? observedArgs = null;
        adapter.CollectionChanged += (_, e) => observedArgs = e;

        Assert.DoesNotThrow(() => adapter.Clear());

        Assert.That(adapter, Is.Empty);
        Assert.That(observedArgs, Is.Not.Null);
        Assert.That(observedArgs!.OldItems, Is.Not.Null);
        Assert.That(observedArgs.OldItems!.Cast<string>(), Is.EquivalentTo(new[] { "a", "b" }));
    }

    /// <summary>
    /// Tests that clearing an already-empty collection doesn't throw and raises no event, since
    /// nothing changed.
    /// </summary>
    [Test]
    public void ClearOnEmptyCollectionDoesNotThrowOrRaiseEvent()
    {
        var adapter = new CollectionAdapter<string, string>(new List<string>());
        var eventRaised = false;
        adapter.CollectionChanged += (_, _) => eventRaised = true;

        Assert.DoesNotThrow(() => adapter.Clear());

        Assert.That(eventRaised, Is.False);
    }
}

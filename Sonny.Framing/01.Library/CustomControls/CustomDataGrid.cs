// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ListBox = System.Windows.Controls.ListBox;

namespace SonnyBIM;

public class CustomDataGrid
{

    [Obfuscation]
    public static readonly DependencyProperty IsSubscribedToSelectionChangedProperty =
        DependencyProperty.RegisterAttached(
            "IsSubscribedToSelectionChanged",
            typeof(bool),
            typeof(CustomDataGrid),
            new PropertyMetadata(default(bool)));


    public static void SetIsSubscribedToSelectionChanged(DependencyObject element, bool value)
    {
        element.SetValue(IsSubscribedToSelectionChangedProperty, value);
    }

    public static bool GetIsSubscribedToSelectionChanged(DependencyObject element)
    {
        return (bool)element.GetValue(IsSubscribedToSelectionChangedProperty);
    }

    [Obfuscation]
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(CustomDataGrid),
            new PropertyMetadata(default(IList), OnSelectedItemsChanged));

    [Obfuscation]
    public static void SetSelectedItems(DependencyObject element, IList value)
    {
        element.SetValue(SelectedItemsProperty, value);
    }

    [Obfuscation]
    public static IList GetSelectedItems(DependencyObject element)
    {
        return (IList)element.GetValue(SelectedItemsProperty);
    }

    [Obfuscation]
    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (!(d is ListBox || d is MultiSelector)) {
            throw new ArgumentException(
                "Somehow this got attached to an object I don't support. ListBoxes and Multiselectors (DataGrid), people. Geesh =P!");
        }

        var selector = (Selector)d;
        var oldList = e.OldValue as IList;
        if (oldList != null)
        {
            var obs = oldList as INotifyCollectionChanged;
            if (obs != null)
            {
                obs.CollectionChanged -= OnCollectionChanged;
            }

            // If we're orphaned, disconnect lb/dg events.
            if (e.NewValue == null)
            {
                selector.SelectionChanged -= OnSelectorSelectionChanged;
                SetIsSubscribedToSelectionChanged(selector, false);
            }
        }

        var newList = (IList)e.NewValue;
        if (newList != null)
        {
            var obs = newList as INotifyCollectionChanged;
            if (obs != null)
            {
                obs.CollectionChanged += OnCollectionChanged;
            }

            PushCollectionDataToSelectedItems(newList, selector);
            var isSubscribed = GetIsSubscribedToSelectionChanged(selector);
            if (!isSubscribed)
            {
                selector.SelectionChanged += OnSelectorSelectionChanged;
                SetIsSubscribedToSelectionChanged(selector, true);
            }
        }
    }

    [Obfuscation]
    private static void PushCollectionDataToSelectedItems(IList obs, DependencyObject selector)
    {
        var listBox = selector as ListBox;
        if (listBox != null)
        {
            if (obs.Count > 0)
            {
                listBox.SelectedItems.Clear();
                foreach (var ob in obs)
                {
                    listBox.SelectedItems.Add(ob);
                }
            }
            else
            {
                foreach (var ob in listBox.SelectedItems)
                {
                    obs.Add(ob);
                }
            }

            return;
        }

        // Maybe other things will use the multiselector base... who knows =P
        var grid = selector as MultiSelector;
        if (grid != null)
        {
            if (obs.Count > 0)
            {
                grid.SelectedItems.Clear();
                foreach (var ob in obs)
                {
                    grid.SelectedItems.Add(ob);
                }
            }
            else
            {
                foreach (var ob in grid.SelectedItems)
                {
                    obs.Add(ob);
                }
            }

            return;
        }

        throw new ArgumentException(
            "Somehow this got attached to an object I don't support. ListBoxes and Multiselectors (DataGrid), people. Geesh =P!");
    }

    [Obfuscation]
    private static void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dep = (DependencyObject)sender;
        var items = GetSelectedItems(dep);
        var col = items as INotifyCollectionChanged;

        // Remove the events so we don't fire back and forth, then re-add them.
        if (col != null)
        {
            col.CollectionChanged -= OnCollectionChanged;
        }

        try
        {
            foreach (var oldItem in e.RemovedItems)
                items.Remove(oldItem);
        }
        catch (Exception exception)
        {
        }

        try
        {
            foreach (var newItem in e.AddedItems)
                items.Add(newItem);
        }
        catch (Exception exception)
        {
        }



        if (col != null)
        {
            col.CollectionChanged += OnCollectionChanged;
        }
    }


    [Obfuscation]
    private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Push the changes to the selected item.
        var listbox = sender as ListBox;
        if (listbox != null)
        {
            listbox.SelectionChanged -= OnSelectorSelectionChanged;
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                listbox.SelectedItems.Clear();
            }
            else
            {
                foreach (var oldItem in e.OldItems)
                {
                    listbox.SelectedItems.Remove(oldItem);
                }

                foreach (var newItem in e.NewItems)
                {
                    listbox.SelectedItems.Add(newItem);
                }
            }

            listbox.SelectionChanged += OnSelectorSelectionChanged;
        }

        var grid = sender as MultiSelector;
        if (grid != null)
        {
            grid.SelectionChanged -= OnSelectorSelectionChanged;
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                grid.SelectedItems.Clear();
            }
            else
            {
                foreach (var oldItem in e.OldItems)
                {
                    grid.SelectedItems.Remove(oldItem);
                }

                foreach (var newItem in e.NewItems)
                {
                    grid.SelectedItems.Add(newItem);
                }
            }

            grid.SelectionChanged += OnSelectorSelectionChanged;
        }
    }

}

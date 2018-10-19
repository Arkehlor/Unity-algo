using System;

// Data Structure used to speed up the choosing process of the best node
public class Heap<T> where T : IHeapItem<T>
{

    T[] items; // Simple array of T
    int currentItemCount; // Number of elements in the array

    // Basic constructor
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    // Adds an element to the heap
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount; // Modifies the index of the soon to be stored item
        items[currentItemCount] = item; // Stores the item at the last location of the array
        SortUp(item); // Sorts the heap to get the newly added item to the correct location
        currentItemCount++; // Increments the number of items in the heap
    }

    // Returns the first element of the heap
    public T RemoveFirst()
    {
        T firstItem = items[0]; // Stores the first item
        currentItemCount--; // Decreases the number of items in the heap
        items[0] = items[currentItemCount]; // Get the last element of the heap to the front
        items[0].HeapIndex = 0; // Modifies the index of the now first element
        SortDown(items[0]); // Rebalances the heap
        return firstItem; // Returns the initial first item
    }

    // Correctly replace the item through the heap
    public void UpdateItem(T item)
    {
        SortUp(item); // For now only checks for earlier elements, might want to check for later ones if the data structure is used for something else
    }

    // Number of items in the heap
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    // Verifies whether an item is in the heap or not
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    // Replaces the item by checking for children items
    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1; // Computes the indexes of the children items
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount) // Checks if the item has children
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount) // Checks if the item has 2 children
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0 ) // Checks which child is the best
                        swapIndex = childIndexRight; // Stores the index of the best child

                if (item.CompareTo(items[swapIndex]) < 0) // If the item is worth swapping with one of its children
                {
                    Swap(item, items[swapIndex]); // Swaps it
                }
                else // If the item is not worth sorting down
                {
                    return; // Exits
                }
            }
            else // If the item has no children
            {
                return; // Exits
            }
        }
    }

    // Replaces the items by checking for parent items
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2; // Computes the index of the parent item

        while (true)
        {
            T parentItem = items[parentIndex]; // Gets the parent item
            if (item.CompareTo(parentItem) > 0) // If the item is worth swapping with its parent
            {
                Swap(item, parentItem); // Swaps it
            }
            else // If the item is not worth swapping
            {
                break; // Exits
            }

            parentIndex = (item.HeapIndex - 1) / 2; // Computes the index of the next parent item
        }
    }

    // Swaps the position of two items in the heap
    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB; // Swaps the items 
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex; // Swaps the indexes
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

// Interface needed to be implemented to be useable by this class
public interface IHeapItem<T> : IComparable<T> // Makes use of the CompareTo function so this dependancy is needed
{
    int HeapIndex
    {
        get;
        set;
    }
}
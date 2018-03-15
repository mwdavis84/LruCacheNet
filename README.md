# LruCacheNet

A fast, generic, thread-safe Least Recently Used (LRU) cache for .NET Standard.

# Usage

1) Install the Nuget package from https://www.nuget.org/packages/LruCacheNet/1.0.0

2) Add the namespace 
    `using LruCacheNet;`
    
3) Create an instance of the cache, with the type of data you'd like to store

    ```
    // Create a cache, optionally with a capacity
    LruCache<object> cache = new LruCache<object>(); 
    
    // Add an item to the front of the cache
    cache.AddOrUpdate("Key", new object());          
    
    // Retrieve the item from the cache and move it to the front
    object retrieved = cache.Get("Key");             
    
    // Move an item to the front of the cache
    cache.Refresh("Key");               
    
    // Check if an item is in the cache without affecting its position
    bool exists = cache.ContainsKey("Key");
    
    // Retrieve an item from the cache without affecting its position
    object retrieved = cache.Peek("Key");
    
    // Remove an item from the cache
    cache.Remove("Key");
    
    // Clear the cache
    cache.Clear();
    ```
    
You can also set a custom method to update an item if its already in the cache, or create a copy when adding.

    
    LruCache<MyData> cache = new LruCache<MyData>();
    cache.SetUpdateMethod((cachedData, updateData) =>
    {
       if (cachedData.MyField != updateData.MyField)
       {
          cachedData.MyField = updateData.MyField;
          return true;
       }
       else
       {
          return false;
       }
    });
    cache.SetCopyMethod((newData) =>
    {
       return new MyData
       {
          MyField = newData.MyField;
       }
    });
    

# LruCacheNet

A fast, generic, thread-safe Least Recently Used (LRU) cache for .NET Standard.

# Usage

1) Install the Nuget package from https://www.nuget.org/packages/LruCacheNet

2) Add the namespace 
    `using LruCacheNet;`
    
3) Create an instance of the cache, with the type of data you'd like to store

    ```
    // Create a cache, optionally with a capacity
    LruCache<string, object> cache = new LruCache<string, object>(); 
    
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
    
    // Get an ordered list of data from most recently used item to least
    List<object> items = cache.ToList();
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
    
You can use the LruCache like a regular ol' dictionary, if you'd like.

    IDictionary<int, int> data = new LruCache();
    data[0] = 5;
    Console.WriteLine(data[0] == 5);
    foreach (int key : in data.Keys)
    {
       // Do something fun but don't change the cache!
    }

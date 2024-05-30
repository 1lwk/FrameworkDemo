using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MyCollection:IEnumerable<int>
{
    private int[] _items;
    public MyCollection(int[] _items) 
    {
        this._items = _items;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return new MyEnumerator(_items);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class MyEnumerator : IEnumerator<int>
{
    private int[] _items;
    private int _postion = -1;

    public MyEnumerator(int[] _items)
    {
        this._items = _items;
    }

    public int Current
    {
        get
        {
            if(_postion<0||_postion>=_items.Length)
            {
                throw new InvalidOperationException();
            }
            return _items[_postion];
        } 
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        
    }

    public bool MoveNext()
    {
        _postion++;
        return _postion < _items.Length;
    }

    public void Reset()
    {
        _postion = -1;
    }
}

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var items = new int[] { 1, 2, 3, 4, 5 };
        var collection=new MyCollection(items);
        foreach (var item in collection)
        {
            Debug.Log(item);
        }
        Debug.Log("更改提交");
        StartCoroutine(RunTest());
    }

    IEnumerator RunTest()
    {
        yield return new WaitForSeconds(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

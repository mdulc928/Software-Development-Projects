using System;

public class ArrayIter<T> : Iter<T> {
    private T [] theList;
    private int index;

    public ArrayIter(T[] _theList) {
        theList = _theList;
        index = 0;
    }
	
    public ArrayIter(T[] _theList, int _index) {
        theList = _theList;
        index = _index;
    }

    public ArrayIter(ArrayIter<T> toCopy) {
        theList = toCopy.theList;
	index = toCopy.index;
    }

    public void next() { ++index; }
    public T get() { return theList[index]; }
    public void set(T value) { theList[index] = value; }
   
    public Iter<T> copy() {
        return new ArrayIter<T>(this);
    }
   
    public bool isEqualTo(Iter<T> other) { 
        if (other.GetType() == typeof(ArrayIter<T>)) {
   	    try {
                ArrayIter<T> _other = (ArrayIter<T>) other;
   	        return (theList == _other.theList && index == _other.index);
   	    } catch (InvalidCastException) { return false; }
   	} else { return false; }
    }
}
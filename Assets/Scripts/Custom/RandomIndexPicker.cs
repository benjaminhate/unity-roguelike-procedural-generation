using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomIndexPicker <T> {

	List<T> list;
	List<int> indexes;

	public RandomIndexPicker(List<T> list){
		this.list = list;
		indexes = new List<int> ();
		for (int i = 0; i < list.Count; i++) {
			indexes.Add (i);
		}
	}

	public int PickIndex(bool remove = true){
		int index = indexes [Random.Range (0, indexes.Count)];
		Debug.Log (index);
		Print ();
		if (remove)
			RemoveAt (index);
		return index;
	}

	public T Pick(bool remove = true){
		int index = PickIndex (remove);
		return list [index];
	}

	public int Count(){
		return indexes.Count;
	}

	public bool IsEmpty(){
		return indexes.Count <= 0;
	}

	private void RemoveAt(int index){
		indexes.Remove (index);
	}

	private void Print(){
		Debug.Log ("Count : " + Count ());
		string str = "Indexes : ";
		foreach (int i in indexes) {
			str += i.ToString () + " ";
		}
		Debug.Log (str);
		str = "Values : ";
		foreach (int i in indexes) {
			str += list[i].ToString () + " ";
		}
		Debug.Log (str);
	}
}

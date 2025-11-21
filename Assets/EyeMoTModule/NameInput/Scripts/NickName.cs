using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickName : MonoBehaviour
{
    private static string value;
    private string[] init_Names = new string[] { "Lion", "Zebra", "Horse", "Tiger" };

    // Start is called before the first frame update
    void Start()
    {
        string init_name = init_Names[Random.Range(0, init_Names.Length)];
        if(value == "")
            value = init_name;
        FindObjectOfType<NameInput>().Init(value);
    }

    public void Set_Value(string name)
    {
        value = name;
    }

    //    public static string Get_Value()
    //    {
    //        string[] init_names = new string[] { "Lion", "Zebra", "Horse", "Tiger" };
    //        string init_name = init_names[Random.Range(0, init_names.Length)];
    //        return (value is string _value) ? _value : SaveGame.Load<string>(Key, init_name);
    //    }
}

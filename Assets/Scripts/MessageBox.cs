using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public string MyProperty
    {
        get
        {
            return Text.text;
        }
        set
        {
            Text.text = value;
        }
    }

    private Text Text;

    private void Awake()
    {
        Text = GetComponentInChildren(typeof(Text)) as Text;
    }

    public void OkButton()
    {
        Destroy(this.gameObject);
    }
}

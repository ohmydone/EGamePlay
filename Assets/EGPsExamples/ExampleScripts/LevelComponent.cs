using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelComponent : MonoBehaviour
{
    public Text ExpText;
    
    public int Exp=0;
    public int Level=1;
    public int EachLevelExp = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ExpText.text = $"Exp:{Exp}  Level:{Level}";
    }

    public void AddExp(int exp)
    {
        Exp += exp;
        if (Exp >= EachLevelExp)
        {
            Level += 1;
            Exp -= EachLevelExp;
        }
    }
}

using PZPK.Core;
using PZPK.Core.Packing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZPK.Desktop.Main.Creator;

public record CreateProperties
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string Password { get; set; } = "";
    public int BlockSize { get; set; } = Constants.Sizes.t_4KB;

    public bool Check()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            return false;
        }

        if (BlockSize % 1024 != 0)
        {
            return false;
        }

        return true;
    }
}

internal class CreatorModel
{
    public IndexCreator Index;
    public CreateProperties Properties;

    public int Step = 1; //1:Index 2:Properties 3:Packing 4:Complete

    public CreatorModel()
    {
        Index = new IndexCreator();
        Properties = new CreateProperties();
    }

    public void NextStep()
    {
        if (Step == 1)
        {
            if (!Index.IsEmpty) Step++;
        }
        else if (Step == 2)
        {
            if (Properties.Check()) Step++;
        }
        else if (Step == 3)
        {
            Step++;
        }
    }

    public void Reset()
    {
        Index = new IndexCreator();
        Properties = new CreateProperties();
        Step = 1;
    }
}

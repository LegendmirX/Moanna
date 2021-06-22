using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;

public class SpriteManager : MonoBehaviour 
{
    
    public enum SpriteCatagory
    {
        Tiles,
        InstalledObjects,
        Inventory,
        Player,
        Zombie,
        Spawns,
        Icon
    }

    public enum SpriteRotation
    {
        Forward,
        Left,
        Backward,
        Right
    }
    public int currRotation = 0;
    public int enumLength = Enum.GetValues(typeof(SpriteRotation)).Length;

    static public SpriteManager current;

    Dictionary<string, Sprite> tileSprites;
    Dictionary<string, Texture2D> tileTextures;
    Dictionary<string, Sprite> installedObjectSprites;
    Dictionary<string, Sprite> inventorySprites;
    Dictionary<string, Sprite> playerSprites;
    Dictionary<string, Sprite> corpses;
    Dictionary<string, Sprite> spawnSprites;
    Dictionary<string, Sprite> inventoryIcons;

    Dictionary<string, RuntimeAnimatorController> ZombieAnimators;
    RuntimeAnimatorController PlayerAnimator;

    Sprite ErrorSprite;

    void Awake()
    {
        
    }

    void OnEnable()
    {

    }

    public List<Action> SetUp()
    {
        current = this;

        List<Action> actions = new List<Action>();
        actions.Add(LoadSprites);
        actions.Add(LoadResouceIconSprites);
        actions.Add(LoadResoucePlayerSprites);
        actions.Add(LoadResouceCorpseSprites);
        actions.Add(LoadResouceTileSprites);

        /*
        actions.Add( LoadTileSprites );
        actions.Add( LoadInstalledObjectSprites );
        actions.Add( LoadInventorySprites );
        actions.Add( LoadSpawnerSprites );
        */

        return actions;
    }

    void LoadSprites()
    {
        Debug.Log("LoadSprites:");
        string filePath = LoadImgsPath();
        
        ZombieAnimators = new Dictionary<string, RuntimeAnimatorController>();

        ErrorSprite = (Sprite)Resources.Load<Sprite>("Imgs/Pink_1x1");

        PlayerAnimator = (RuntimeAnimatorController)Resources.Load<RuntimeAnimatorController>("Animations/PlayerAnimator/PlayerVisuals") as RuntimeAnimatorController;

        foreach (RuntimeAnimatorController anim in Resources.LoadAll<RuntimeAnimatorController>("Animations/EnemyAnimator"))
        {
            ZombieAnimators.Add(anim.name, anim);
            //Debug.Log(anim.name);
        }
    }

    #region HelperFuncs   
    string LoadImgsPath()
    {
        return "Imgs";
    }

    public int GetSpriteListLength(SpriteCatagory cat)
    {
        int length = 0;

        switch (cat)
        {
            case SpriteCatagory.Tiles:
                length = tileSprites.Count;
                break;
            case SpriteCatagory.InstalledObjects:
                length = installedObjectSprites.Count;
                break;
            case SpriteCatagory.Inventory:
                length = inventorySprites.Count;
                break;
            case SpriteCatagory.Player:
                length = playerSprites.Count;
                break;
            case SpriteCatagory.Zombie:
                length = corpses.Count;
                break;
        }

        return length;
    }

    public List<string> GetSpriteNames(SpriteCatagory cat)
    {
        List<string> NameList = new List<string>();

        switch (cat)
        {
            case SpriteCatagory.Tiles:
                foreach(string name in tileSprites.Keys)
                {
                    NameList.Add(name);
                }
                break;
            case SpriteCatagory.InstalledObjects:
                foreach (string name in installedObjectSprites.Keys)
                {
                    NameList.Add(name);
                }
                break;
            case SpriteCatagory.Inventory:
                foreach (string name in inventorySprites.Keys)
                {
                    NameList.Add(name);
                }
                break;
            case SpriteCatagory.Player:
                foreach (string name in playerSprites.Keys)
                {
                    NameList.Add(name);
                }
                break;
            case SpriteCatagory.Zombie:
                foreach(string name in corpses.Keys)
                {
                    NameList.Add(name);
                }
                break;
        }

        return NameList;
    }

	public Sprite GetSprite(SpriteCatagory cat, string spriteName)
    {
        Sprite sprite = null;

        switch(cat)
        {
            case SpriteCatagory.Tiles:
		        if(tileSprites.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - TileSprites: Dose not contain " + spriteName);
			        sprite = ErrorSprite;
		        }
                else
                {
                    sprite = tileSprites[spriteName];
                }
                break;
            case SpriteCatagory.InstalledObjects:
                if (installedObjectSprites.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - InstalledObjectSprites: Dose not contain " + spriteName);
                    sprite = ErrorSprite;
                }
                else
                {
                    sprite = installedObjectSprites[spriteName];
                }
                break;
            case SpriteCatagory.Inventory:
                if (inventorySprites.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - InventorySprites: Dose not contain " + spriteName);
                    sprite = ErrorSprite;
                }
                else
                {
                    sprite = inventorySprites[spriteName];
                }
                break;
            case SpriteCatagory.Player:
                Debug.LogError("NotImplimented");
                break;
            case SpriteCatagory.Zombie:
                if(corpses.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - enemySprites: Dose not contain " + spriteName);
                    sprite = ErrorSprite;
                }
                else
                {
                    sprite = corpses[spriteName];
                }
                break;
            case SpriteCatagory.Spawns:
                if (spawnSprites.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - spawnSprites: Dose not contain " + spriteName);
                    sprite = ErrorSprite;
                }
                else
                {
                    sprite = spawnSprites[spriteName];
                }
                break;
            case SpriteCatagory.Icon:
                if (inventoryIcons.ContainsKey(spriteName) == false)
                {
                    Debug.LogError("SpriteManager - spawnSprites: Dose not contain " + spriteName);
                    sprite = ErrorSprite;
                }
                else
                {
                    sprite = inventoryIcons[spriteName];
                }
                break;

        }
		return sprite;
	}

    public Texture2D GetTexture(SpriteCatagory cat, string spriteName)
    {
        Texture2D texture = null;

        switch (cat)
        {
            case SpriteCatagory.Tiles:
                if (tileSprites.ContainsKey(spriteName) == false)
                {
                    //Debug.LogError("SpriteManager - TileSprites: Dose not contain " + spriteName);
                    texture = ErrorSprite.texture;
                }
                else
                {
                    texture = tileTextures[spriteName];
                }
                break;
        }
        return texture;
    }

    public RuntimeAnimatorController GetAnimator(SpriteCatagory cat, string animType)
    {
        RuntimeAnimatorController controller = null;
        switch (cat)
        {
            case SpriteCatagory.Player:
                controller = PlayerAnimator;
                break;
            case SpriteCatagory.Zombie:
                if(ZombieAnimators.ContainsKey(animType) == false)
                {
                    Debug.Log("ZombieAnimators dose not contail " + animType);
                }
                else
                {
                    controller = ZombieAnimators[animType];
                }
                break;
        }
        return controller;
    }
    #endregion

    #region LoadSpriteFuncs

    #region LoadFromResources
    void LoadResoucePlayerSprites()
    {
        Debug.Log("-PlayerSprites");
        playerSprites = new Dictionary<string, Sprite>();

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Player", typeof(Sprite)))
        {
            playerSprites.Add(loadedsprite.name, loadedsprite);
        }
    }

    void LoadResouceCorpseSprites()
    {
        Debug.Log("-CorpseSprites");
        corpses = new Dictionary<string, Sprite>();

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Corpses", typeof(Sprite)))
        {
            corpses.Add(loadedsprite.name, loadedsprite);
        }
    }

    void LoadResouceIconSprites()
    {
        Debug.Log("-IconSprites");
        inventoryIcons = new Dictionary<string, Sprite>();

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Icons", typeof(Sprite)))
        {
            inventoryIcons.Add(loadedsprite.name, loadedsprite);
        }
    }

    void LoadResouceTileSprites()
    {
        Debug.Log("-TileSprites");
        tileSprites = new Dictionary<string, Sprite>();

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Tiles", typeof(Sprite)))
        {
            tileSprites.Add(loadedsprite.name, loadedsprite);
        }
    }

    void LoadResouceInstalledObjectSprites()
    {
        Debug.Log("-InstalledObjectSprites");
        installedObjectSprites = new Dictionary<string, Sprite>();

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/InstalledObjects", typeof(Sprite)))
        {
            installedObjectSprites.Add(loadedsprite.name, loadedsprite);
        }
    }

    #endregion

    #region LoadFromStreamingAssets
    void LoadTileSprites()
    {
        Debug.Log("-TileSprites:");
        string filePath = LoadImgsPath();

        string path = Path.Combine(filePath, "Tiles");
        tileSprites = new Dictionary<string, Sprite>();

        List<Sprite> sprites = GetSprites(path);

        if(sprites != null && sprites.Count > 0)
        {
            foreach (Sprite s in sprites)
            {
                tileSprites.Add(s.name, s);
            }
        }
        else
        {
            Debug.LogError("Somthing went terribly wrong");
        }
    }

    void LoadInstalledObjectSprites()
    {
        Debug.Log("-InstalledObjectSprites");
        string filePath = LoadImgsPath();

        string path = Path.Combine(filePath, "InstalledObjects");
        installedObjectSprites = new Dictionary<string, Sprite>();

        List<Sprite> sprites = GetSprites(path);

        if (sprites != null && sprites.Count > 0)
        {
            foreach (Sprite s in sprites)
            {
                installedObjectSprites.Add(s.name, s);
            }
        }
        else
        {
            Debug.LogError("Somthing went terribly wrong");
        }
    }

    void LoadInventorySprites()
    {
        Debug.Log("-InventorySprites");
        string filePath = LoadImgsPath();

        string path = Path.Combine(filePath, "Inventory");
        inventorySprites = new Dictionary<string, Sprite>();

        List<Sprite> sprites = GetSprites(path);

        if (sprites != null && sprites.Count > 0)
        {
            foreach (Sprite s in sprites)
            {
                inventorySprites.Add(s.name, s);
            }
        }
        else
        {
            Debug.LogError("Somthing went terribly wrong");
        }
    }

    void LoadSpawnerSprites()
    {
        Debug.Log("-SpawnerSprites");
        string filePath = LoadImgsPath();

        string path = Path.Combine(filePath, "Spawner");
        spawnSprites = new Dictionary<string, Sprite>();

        List<Sprite> sprites = GetSprites(path);

        if (sprites != null && sprites.Count > 0)
        {
            foreach (Sprite s in sprites)
            {
                spawnSprites.Add(s.name, s);
            }
        }
        else
        {
            Debug.LogError("Somthing went terribly wrong");
        }

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Spawner", typeof(Sprite)))
        {
            spawnSprites.Add(loadedsprite.name, loadedsprite);
        }
    }

    void LoadInventoryIcons()
    {
        Debug.Log("-Icons");
        string filePath = LoadImgsPath();

        string path = Path.Combine(filePath, "Icons");
        inventoryIcons = new Dictionary<string, Sprite>();

        List<Sprite> sprites = GetSprites(path);

        if (sprites != null && sprites.Count > 0)
        {
            foreach (Sprite s in sprites)
            {
                inventoryIcons.Add(s.name, s);
            }
        }
        else
        {
            Debug.LogError("Somthing went terribly wrong");
        }

        foreach (Sprite loadedsprite in Resources.LoadAll("Imgs/Icons", typeof(Sprite)))
        {
            inventoryIcons.Add(loadedsprite.name, loadedsprite);
        }
    }
    #endregion

    #endregion

    #region LoadFromDisk
    List<Sprite> GetSprites(string filePath)
    {
        List<Sprite> sprites = new List<Sprite>();

        string[] filesInDir = BetterStreamingAssets.GetFiles(filePath); //gets link to each file in the folder
        foreach(string fn in filesInDir)
        {
            //Debug.Log(fn);
            if (fn.Contains(".xml") || fn.Contains(".meta"))
            {
                continue;
            }
            else
            {
                Texture2D texture = LoadTexture(fn);
                texture.filterMode = FilterMode.Point;
                if (texture == null) //Did load texture work?
                {
                    Debug.Log("Not an img");
                }
                else
                {
                    string xmlPath = FindSpriteXml(fn);
                    if (xmlPath != null) //its multiple imgs in one texture
                    {
                        string xmlText = BetterStreamingAssets.ReadAllText(xmlPath); 
                        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));
                        if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                        {
                            do
                            {
                                sprites.Add(ReadSpriteFromXml(reader, texture));
                            }
                            while (reader.ReadToNextSibling("Sprite"));
                        }
                        else
                        {
                            Debug.LogError("No <Sprites> tag found. should this file be in imgs?");
                        }
                    }
                    else //assume its a single img
                    {
                        string name = Path.GetFileNameWithoutExtension(fn);
                        sprites.Add(LoadSprite(name, texture, new Rect(0, 0, texture.width, texture.height), texture.width));
                    }
                }
            }
        }
        return sprites;
    }

    Texture2D LoadTexture(string filePath)
    {
        byte[] imageBytes = BetterStreamingAssets.ReadAllBytes(filePath); //reading bytes from imgs

        Texture2D imageTexture = new Texture2D(2, 2); //dummy texture
        //attempting to load texture from bytes, overwriting the dummy texture
        if (imageTexture.LoadImage(imageBytes))
        {
            return imageTexture;
        }
        return null;
    }

    string FindSpriteXml(string filePath)
    {
        string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
        string basePath = Path.GetDirectoryName(filePath);

        string xmlPath =  Path.Combine(basePath, baseSpriteName + ".xml");

        if (BetterStreamingAssets.FileExists(xmlPath))
        {
            return xmlPath;
        }

        return null;
    }

    Sprite ReadSpriteFromXml(XmlReader reader, Texture2D texture)
    {
        reader.MoveToAttribute("Name");
        string name = reader.ReadContentAsString();
        reader.MoveToAttribute("X");
        int X = reader.ReadContentAsInt();
        reader.MoveToAttribute("Y");
        int Y = reader.ReadContentAsInt();
        reader.MoveToAttribute("W");
        int W = reader.ReadContentAsInt();
        reader.MoveToAttribute("H");
        int H = reader.ReadContentAsInt();
        reader.MoveToAttribute("pixelsPerUnit");
        int pixelsPerUnit = reader.ReadContentAsInt();

        float calc = pixelsPerUnit / 2f;
        float pivX = (1f/W) * calc;
        float pivY = (1f/H) * calc;

        return LoadSprite(name, texture, new Rect(X, Y, W, H), pixelsPerUnit, pivX, pivY);
    }

    Sprite LoadSprite(string name, Texture2D imageTexture, Rect coords, int pixelsPerUnit, float pivPointX = 0.5f, float pivPointY = 0.5f)
    {
        Vector2 pivitPoint = new Vector2(pivPointX, pivPointY);

        Sprite sprite = Sprite.Create(imageTexture, coords, pivitPoint, pixelsPerUnit);
        sprite.name = name;

        return sprite;
    }
    #endregion
}

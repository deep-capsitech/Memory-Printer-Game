using System;
using System.Collections.Generic;

[Serializable]
public class JsonLevelData
{
    public List<JsonLevel> levels;
}

[Serializable]
public class JsonLevel
{
    public int level;
    public float levelTime = 60f;
    public float mapChangeTime = 20f;
    public List<JsonLayout> layouts;
}

[Serializable]
public class JsonLayout
{
    public JsonBooster booster;
    public List<JsonObstacle> obstacles;
}

[Serializable]
public class JsonObstacle
{
    public int tileX;
    public int tileZ;
}

[Serializable]
public class JsonBooster
{
    public int tileX;
    public int tileZ;
}

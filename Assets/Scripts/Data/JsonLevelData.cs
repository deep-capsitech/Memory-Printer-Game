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
    public float levelTime;
    public float mapChangeTime;
    public float snapshotTime;
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

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Note: Will only work when manually triggering "Reimport" on a file, not when importing a file to Unity for the first time.
/// In order for this to work the grandparent directory must be names "AutoProcessedSpriteSheets"
/// The immediate parent directory must be named the desired horizontal tile size of the sliced spritesheet in pixels, plus underscore...
/// plus the desired horizontal tile size of the sliced spritesheet in pixels, plus underscore...
/// plus the desired pixels per unit
///     eg. "16__12_100"
/// </summary>
public class SpriteProcessor : AssetPostprocessor
{
    bool continueProcess = false;
    int ppu;
    int spriteSizeX;
    int spriteSizeY;
    Vector2 customPivot;
    //float xAxisDivisor = 0.5f;
    float yAxisDivisor = 0.0f;

    void OnPreprocessTexture()
    {
        if (Directory.GetParent(assetPath).Parent.Name != "AutoProcessedSpriteSheets") return;

        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.isReadable = true;
        textureImporter.textureType = TextureImporterType.Sprite;

        TextureImporterSettings importerSettings = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(importerSettings);
        importerSettings.spriteMeshType = SpriteMeshType.Tight;
        importerSettings.spriteMode = (int)SpriteImportMode.Multiple;
        textureImporter.SetTextureSettings(importerSettings);

        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.maxTextureSize = 2048;
        textureImporter.alphaIsTransparency = true;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.mipmapEnabled = false;
        textureImporter.filterMode = FilterMode.Point;

        string[] parentDir = Directory.GetParent(assetPath).Name.Split('_');

        //If we can't parse the parent directory values as integers, don't continue processing the sprite
        if (System.Int32.TryParse(parentDir[0], out spriteSizeX) &&
        System.Int32.TryParse(parentDir[1], out spriteSizeY) &&
        System.Int32.TryParse(parentDir[2], out ppu)) {
            customPivot = new Vector2(spriteSizeX, spriteSizeY).normalized / 2; // defaults to half, (0.5, 0.5)

            int x = 2;
            int n = Mathf.RoundToInt(Mathf.Log(ppu, x)) + 1; // +1 para que divida por el logaritmo en base x (la cantidad de veces que queremos dividirlo), que daría en total 1, y divida una vez más asi queda en la mitad
            Debug.Log(spriteSizeX + " dividido " + n + " veces en " + x + " es " + DividedNTimesByX(spriteSizeX, x, n));
            customPivot = new Vector2(DividedNTimesByX(spriteSizeX, x, n), customPivot.y * yAxisDivisor);

            textureImporter.spritePixelsPerUnit = ppu;
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
            continueProcess = true;
        }
    }

    public void OnPostprocessTexture(Texture2D texture) {
        //If we failed requirement during the PreProcess stage, return
        if (!continueProcess) return;

        /*
        if (Mathf.IsPowerOfTwo((int)customPivot.x) == false || Mathf.IsPowerOfTwo((int)customPivot.y) == false) {

        }

        if (spriteSizeX == spriteSizeY && Mathf.IsPowerOfTwo(spriteSizeX)) {
            if (customPivot.x % 2 == 0 || customPivot.x % 2 == 0) {
            
            }
        }
        */

        if (spriteSizeX > 0 && spriteSizeY > 0) {
            int colCount = texture.width / spriteSizeX;
            int rowCount = texture.height / spriteSizeY;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            int i = 0;

            List<SpriteMetaData> metas = new List<SpriteMetaData>();

            for (int r = rowCount - 1; r > -1; r--) {
                for (int c = 0; c < colCount; c++) {
                    Debug.LogWarning("spriteSizeX = " + spriteSizeX);
                    Debug.LogWarning("spriteSizeY = " + spriteSizeY);

                    SpriteMetaData meta = new SpriteMetaData();
                    meta.rect = new Rect(c * spriteSizeX, r * spriteSizeY, spriteSizeX, spriteSizeY);
                    meta.name = fileName + "_" + i;
                    meta.alignment = (int)SpriteAlignment.Custom;
                    meta.pivot = customPivot;

                    metas.Add(meta);
                    i++;
                    Debug.LogWarning("Pivot Position = " + meta.pivot);
                }
            }
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.spritesheet = metas.ToArray();
            
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
        }
    }

    float DividedNTimesByX(float d, int x, int n) {
        float result = d;
        Debug.Log(n + " es igual al logaritmo de " + d + " en base " + x);
        for (int i = 0; i < n; i++) {
            result /= x;
        }
        return (float) result;
    }
}

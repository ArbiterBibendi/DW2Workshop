using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DynamicCSharp;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad;
using Battlehub.RTEditor;
using DrunkenWrestlers2.Gameplay;
using DrunkenWrestlers2.Networking;
using DW2;

public class ModManager : MonoBehaviour
{
    WorkshopManager workshopManager;
    MapLoader mapLoader;
    PlayerScreen playerScreen;
    RuntimeEditor runtimeEditor;

    string RTEditorGameFolder;
    WorkshopItem[] workshopItems;
    List<WorkshopItem> itemsToLoad;
    FileSystemStorage fileSystemStorage;

    void Start()
    {
        Debug.Log("Starting Script: ModManager");

        fileSystemStorage = new FileSystemStorage("C:/");
        mapLoader = FindObjectOfType<MapLoader>();
        runtimeEditor = FindObjectOfType<RuntimeEditor>();
        playerScreen = FindObjectOfType<PlayerScreen>();
        workshopManager = FindObjectOfType<WorkshopManager>();

        if (workshopManager)
        {
            workshopItems = workshopManager.ListInstalledMods();
        }


        itemsToLoad = new List<WorkshopItem>();
        RTEditorGameFolder = WorkshopManager.MOOAOECCHHL + "Workshop/"; //WorkshopManager.MOOAOECCHHL is a string that holds the path to the runtime editor directory


        //TODO: Add Menu to select mods
        //      Sync loaded mods between players
        //      Check if workshop item is present, if not
        //      subscribe to it
        addMod("Heavy Hits (Dismemberment)"); 
        addMod("Decapitation = Instant Death");
        GameplayManager.OnPlayerDataSpawn += onGameStart;
    }

    

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            
        }
        if (Input.GetKeyDown(KeyCode.U)) //Fix camera in editor
        {
            if (runtimeEditor)
            {
                runtimeEditor.SceneGizmo.gameObject.SetActive(false);
                runtimeEditor.BoxSelect.gameObject.SetActive(false);
                runtimeEditor.Grid.gameObject.SetActive(false);
                RuntimeEditorApplication.SceneCameras[0].gameObject.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) //Close test mode
        {
            RuntimeEditorApplication.IsOpened = true;
        }
    }
    void onGameStart(PlayerData player)
    {
        GameplayManager.OnPlayerDataSpawn -= onGameStart;
        initModsLoad();
    }
    void addMod(string modName)
    {
        foreach (WorkshopItem item in workshopItems)
        {
            if (item.title == modName)
            {
                itemsToLoad.Add(item);
            }
        }
    }
    void initModsLoad()
    {
        foreach (WorkshopItem item in itemsToLoad)
        {
            StorageEventHandler<string, string> itemsCopiedCallback = null;
            if (item.Equals(itemsToLoad.Last()))
            {
                itemsCopiedCallback = new StorageEventHandler<string, string>(onItemsCopied);
            }
            copyFiles(item.contentFolder, RTEditorGameFolder + item.id, itemsCopiedCallback);
        }
    }
    void onItemsCopied(StoragePayload<string, string> storagePayload)
    {
        try
        {
            Dependencies.ProjectManager.LoadProject("Game", new ProjectManagerCallback<ProjectItem>((ProjectItem pi) =>
            {
                foreach (WorkshopItem item in itemsToLoad)
                {
                    ProjectItem projectItem = Dependencies.ProjectManager.Project.Get(string.Concat(new object[] { "Game/Workshop/", item.id, "/", item.sceneFile }));
                    if (projectItem == null)
                    {
                        Debug.Log("PROJECTITEM NULL");
                    }
                    else
                    {
                        ProjectManagerCallback scenesLoadedCallback = null;
                        if (item.Equals(itemsToLoad.Last()))
                        {
                            scenesLoadedCallback = new ProjectManagerCallback(onScenesLoaded);
                        }
                        Debug.Log("Attempting to load " + item.title);
                        Dependencies.ProjectManager.LoadScene(projectItem, scenesLoadedCallback, true);
                    }
                }
            }));
        }
        catch (Exception E)
        {
            Debug.Log(E.ToString());
        }

    }
    void onScenesLoaded()
    {
        if (FindObjectOfType<CustomScript>())
        {
            try
            {
                FindObjectOfType<CustomScript>().RunScript();
            }
            catch (Exception E)
            {
                Debug.Log(E.ToString());
            }
        }
    }
    bool checkFolderExists(string path)
    {
        bool folderExists = false;
        
        fileSystemStorage.CheckFolderExists(path, (StoragePayload<string, bool> sp) =>
        {
            folderExists = sp.Data;
        });
        return folderExists;
    }
    bool checkFileExists(string path)
    {
        bool fileExists = false;
        fileSystemStorage.CheckFileExists(path, (StoragePayload<string, bool> sp) =>
        {
            fileExists = sp.Data;
        });
        return fileExists;
    }
    string[] getFiles(string path)
    {
        string[] files = null;
        fileSystemStorage.GetFiles(path, (StoragePayload<string, string[]> sp) => 
        {
            files = sp.Data;
        }, false);
        return files;
    }
    void copyFiles(string sourcePath, string destPath, StorageEventHandler<string, string> callback)
    {
        if (!checkFolderExists(destPath))
        {
            fileSystemStorage.CreateFolder(destPath, null);
        }
        string[] files = getFiles(sourcePath);
        foreach (string file in files)
        {
            StorageEventHandler<string, string> filesDoneCallback = null;
            if (file.Equals(files.Last()))
            {
                filesDoneCallback = callback;
            }
            if(!checkFileExists(destPath + "/" + file) && checkFileExists(sourcePath + file))
            {
                Debug.Log(sourcePath + file + " " + destPath + "/" + file);
                fileSystemStorage.CopyFile(sourcePath + file, destPath + "/" + file, filesDoneCallback);
            }
            else
            {
                Debug.Log("Error copying files");
            }
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class UnitySceneBackup
{
    const string c_SavePathRoot = "SceneBackup";
    const string c_UnityFileExt = ".unity";

    // Have we loaded the prefs yet?
    private static bool m_prefsLoaded = false;

    private static bool m_enabled = false;

    static UnitySceneBackup()
    {
        LoadPrefs();
		EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    static void LoadPrefs()
    {
        if (!m_prefsLoaded)
        {
            m_enabled = EditorPrefs.GetBool("SceneBackupEnabled", false);
            m_prefsLoaded = true;
        }
    }

	static void OnSceneSaved( Scene scene )
	{
		if ( !m_enabled )
        {
			return;
		}

        string scenePath = GetBackupPath( scene.path );

        if ( !Directory.Exists( scenePath ) )
        {
            Directory.CreateDirectory( scenePath );
        }

        string output_filename = FindNextFilename( scene.path );

		try
		{
			File.Copy( scene.path, output_filename, true );
		}
		catch ( System.Exception e )
		{
			Debug.Log( e.ToString() );
		}
	}

    static Scene FindSceneByPath( string path )
    {
        for ( int i = 0; i < EditorSceneManager.sceneCount; i++ )
        {
            Scene scene = EditorSceneManager.GetSceneAt( i );
            if ( path.CompareTo( scene.path ) == 0 )
            {
                return scene;
            }
        }

        return new Scene();
    }

    static string FindNextFilename( string filename )
    {
        string backupPath = GetBackupPath( filename );

        string raw_filename = Path.GetFileNameWithoutExtension( filename );

        int iterFilename = 0;

        while( true )
        {
            string base_output_filename = raw_filename + "." + iterFilename.ToString() + c_UnityFileExt;
            string output_filename = Path.Combine( backupPath, base_output_filename );
            if ( File.Exists( output_filename ) )
            {
                iterFilename++;
                continue;
            }

            return output_filename;
        }
    }

    static string GetBackupPath( string filename )
    {
        string base_filename = Path.GetFileName( filename );
        string scenePath = filename.Replace( "Assets", c_SavePathRoot ).Replace( base_filename, string.Empty );

        return scenePath;
    }

    [PreferenceItem("Scene Backup")]
    public static void PreferencesGUI()
    {
        LoadPrefs();

        // Preferences GUI
        m_enabled = EditorGUILayout.Toggle("Enable", m_enabled);

        // Save the preferences
        if ( GUI.changed )
        { 
            EditorPrefs.SetBool("SceneBackupEnabled", m_enabled);
        }
    }

}

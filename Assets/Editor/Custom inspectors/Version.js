#pragma strict 
import System.IO;

@script ExecuteInEditMode()

class Version extends EditorWindow {

    @MenuItem ("Window/Version Updater")
	static function ShowWindow() {
		EditorWindow.GetWindow(typeof(Version));
	}
	function OnGUI () {
		EditorGUILayout.LabelField("Game Version: "+GV);
		EditorGUILayout.LabelField("Compile #"+GameVersion);
		EditorGUILayout.LabelField("Full Version: "+ActualVersion);
	}
}

#if UNITY_EDITOR
function MyUpdate () {
	if (EditorApplication.isCompiling) {
		if (!UpdateCheck) {
			UpdateCheck = true;
			OnCompile();
		}
	} else {
		UpdateCheck = false;
	}
}

private var GameVersion : int;
var ActualVersion : String;
private var UpdateCheck = false;
private var GV : String;

function OnEnable() {
	if (File.Exists(Application.dataPath+"/../Version")) {
		var sr = new StreamReader(Application.dataPath+"/../Version");
	    var fileContents = sr.ReadToEnd();
	    sr.Close();
	    var lines = fileContents.Split("\n"[0]);
	    var line1 = fileContents.Split("."[0]);
	    ActualVersion = lines[0];
	    var length = 0;
	    for (line in line1) {
	    	length++;
	    }
	    GameVersion = parseInt(line1[length-1]);
	}
	if (GameObject.FindGameObjectWithTag("GameController")) {
		GV = GameObject.FindGameObjectWithTag("GameController").GetComponent(Game).Version;
		if (GV == "") {
			GV = "Error";
		}
	}
	EditorApplication.update = MyUpdate;
}

function OnCompile() {
	GameVersion++;
	if (GV != "Error") {
		ActualVersion = GV+"."+GameVersion;
		System.IO.File.WriteAllText(Application.dataPath+"/../Version", ActualVersion);
	}
}
#endif
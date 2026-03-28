using UnityEngine;

namespace Guanomancer
{
    public class DocItMd : ScriptableObject
    {
        private const string DEFAULT_MARKDOWN_CONTENT = "# Title\nWrite your **Markdown** here...";
        [HideInInspector] public bool isEditing = false;
        [HideInInspector] public bool isTocFolded = false;
        [HideInInspector] public string headerIconPath;

        [TextArea(10, 30)]
        public string markdownContent = DEFAULT_MARKDOWN_CONTENT;

        public void SetDefaultContent()
        {
            markdownContent = DEFAULT_MARKDOWN_CONTENT;
            if (!string.IsNullOrEmpty(headerIconPath))
            {
                markdownContent = $"{markdownContent}\n![{headerIconPath}]({headerIconPath})";
            }
        }
    }
    /*

    # DocIt MarkDown
    This is the same concept as the DocIt.

    ## About
    Use it to place **readmes and to-dos** directly where the designers and developers need them, documenting **usage, implementation, etc.**

    *When documentation is where you need it, when you need it, you have to put in effort to get things wrong.*
    *..it totally doable though, but you must dedicate yourself to mischif and shenanigans...*
    /Guanomancer, 2026

    ## Formatting
    ### Headers
    # # Top Level
    ## ## Second Level
    ### ### Third Level
    #### #### Fourth Level
    ##### ##### Fifth Level
    ###### ###### Final Level

    ### Line Seperator
    \-\-\- Line seperator
    ---

    ### Font
    \*\* **Bold** \*\*
    \* *Italic* \*
    \~\~ ~~Strike Through~~ \~\~
    < u > <u>Underline</u> < /u >
    < color=#2288ff > <color=#2288ff>Color</color> < /color >
    ### Lists
    - \- Bullet List
    1. 1. Ordered List item 1
    2. 2. Ordered List item 2

    ### To-Do List:
    *[x] \*[x] Do the thing.
    *[ ] \*[ ] Do the other thing.

    ### Images
    ![Alternative Text](Assets/TutorialInfo/Icons/URP.png)

    ### Assets
    [DocIt Markdown Script](Assets/Tools/Docs/DocItMd.cs)

    */
}
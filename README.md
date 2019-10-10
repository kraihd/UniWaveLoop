UniWaveLoop
===
Did you know Unity Audio can handle loop point by default, with no runtime code?

About
---
This editor tool allows you to set a loop point to your wav file in Unity.
Although this tool works on Unity 2018+, the loop points will work on legacy Unity versions, for it modifies not import settings but the wav file itself.

Usage
---
Open the context menu on your wav file in the project window and choose "Open WaveLoop Editor".
Input the sample position and save.

License
---
This tool is under the MIT License.
(However, in most cases, it is not necessary to include this tool itself in your app.)

About（日本語）
---
「Unityの標準オーディオでは、音声の途中からループができない」……そんな誤解を未だに抱いていませんか？
実は、wavファイル自体に定められた特定のメタデータ（smplチャンク）にループポイントを設定してあれば、そこからインポートされたAudioClipは、途中からループをしてくれるのです。
実行時には何の処理も必要ありません。

しかし、音声編集ソフトウェアであっても、smplチャンクを編集できるものは限られています。このツールを利用すれば、UnityEditorから直接wavファイルを編集し、ループポイントを埋め込むことができます。

このツールはUnity2018以上で動作しますが、これで編集されたwavファイルは、以前のバージョンのUnityでもループします。

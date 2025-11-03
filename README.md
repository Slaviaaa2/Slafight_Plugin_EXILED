# 動作環境
EXILED>=`v9.10.1`
# 概要
SCP: Secret LaboratoryのEXILEDフレームワークで動作するプラグインです。<br>
完全に自鯖用に作成しているため、コードの読みやすさ等は全く考慮されていません。<br>
ただ、日本製の最新の資料が途轍もなく少ない為、何かの役に立つかもしれないという考えで公開しました。<br>
作者はC#をいまだ良く分かっていないですし、問題だらけな部分が多いですが少しでもあなたの助けになれば幸いです<br>
（このプラグインにはOmega WarheadやDelta Warheadなどのファンメイド要素が含まれています！なのでconfigをいじったりして利用してみてもいいかもしれません！）<br>
# 利用時の注意点
## BGMについて
Omega WarheadやDelta Warheadイベントの際、BGMが何もしていないと流れません。その為、Configにて音楽ファイルのフォルダパスを指定し、`omega.ogg`,`delta.ogg`というファイル名でBGMをフォルダ内に入れる必要があります。<br>
また、BGM再生時から何秒後に爆発するかもConfigにてご自身の使用するBGMに合わせてfloat形式で設定してください。<br>
因みに、私は[I Dunno氏](https://www.youtube.com/@idunno4670 "(YouTube)")の音楽を使用して制作をしているため、
I Dunno氏の物をダウンロードしてきて設定すれば私が設定した爆発時間をそのまま使う事が出来ます。良かったら試してみてください。<br>
# Config(設定)
ファイルの場所： `C:\Users\[ユーザー名]\AppData\Rooming\EXILED\Configs\Plugins\Slafight_Plugin_EXILED\[サーバーポート].yml`
| Config名                  | 説明                                   | 初期値                                                         | 型     | 
| ------------------------- | -------------------------------------- | -------------------------------------------------------------- | ------ | 
| IsEnabled                 | プラグインを有効化するか               | true                                                           | bool   | 
| Debug                     | コンソールにデバッグログを出力するか   | true                                                           | bool   | 
| AudioReferences           | BGM等の置き場フォルダの指定            | "C:\\Users\\zeros\\AppData\\Roaming\\EXILED\\ServerContents\\" | string | 
| SkeletonSpawnAllowed      | SCP-3114を独自スポーンさせるかどうか   | true                                                           | bool   | 
| SkeletonSpawnChance       | 独自スポーンの確率                     | 0.25f                                                          | float  | 
| WarheadLockAllowed        | AlphaWarheadロックをするかどうか       | true                                                           | bool   | 
| WarheadLockTimeMultiplier | 120秒*設定値秒後にAWをロックさせます。 | 0.75f                                                          | float  | 
| EventAllowed              | 特殊イベントを有効化するかどうか       | true                                                           | bool   | 
| OW_Allowed                | OmegaWarheadを有効化するかどうか       | true                                                           | bool   | 
| OW_BoomTime               | OWの爆発までの時間                     | 160f                                                           | float  | 
| DW_Allowed                | DeltaWarheadを有効化するかどうか       | true                                                           | bool   | 
| DW_BoomTime               | DWの爆発までの時間                     | 100f                                                           | float  | 
| [カスタムアイテム名]      | カスタムアイテムの設定                 | -                                                              | -      | 

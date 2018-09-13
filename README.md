# GetProfile

Eclipse (Varian Medical Systems) で利用できる線量プロファイル取得用のスクリプトです。

v13.7(?)未満ではプラン線量のプロファイルのみ、それ以降ではプラン線量と各ビーム線量のプロファイルを取得します。

プラン線量は処方線量に対する相対値で出力されます。
各ビーム線量はField Normalizationの方法により異なりますが、通常はアイソセンタを100%として出力されます。

## 導入方法

[ここ](https://raw.githubusercontent.com/EuroMediTech/GetProfile/master/GetProfile.cs) を右クリックして「名前を付けてリンク先を保存」するか、画面右上の `Clone or download` からこのリポジトリをダウンロードし、`GetProfile.cs` を取り出します。

EclipseのExternal Beam Planningにて、 `Tools -> Scripts` から `GetProfile.cs` を指定すれば実行されます。

## 使い方

お使いのEclipseのデスクトップに、`input.csv` というファイルを設置します。

[サンプル](https://raw.githubusercontent.com/EuroMediTech/GetProfile/master/input.csv)

この`input.csv`をExcelなどで編集し、線量プロファイルの始点、終点およびプロファイルのstepサイズを**mm単位**で入力します。

|x_0|y_0|z_0|x_1|y_1|z_1|step|
|---|---|---|---|---|---|---|
|-10|0|0|10|0|0|1|

上記の例ですと、(-10, 0, 0)から(10, 0, 0)に向かって、1 mm刻みで線量プロファイルを取得します。

プロファイルは1行ごとにCSVファイルとしてデスクトップに出力され、1行目から順にProfile00.csv, Profile01.csv, ... というファイル名になります。
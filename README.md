# Etupirka

## 概要
エロゲのプレイ時間を統計するツールです。


![](http://i.imgur.com/upkcS2e.png)

## ダウンロード

[Github](https://github.com/Aixile/Etupirka/releases "https://github.com/Aixile/Etupirka/releases")

#### 0.6.0
- Remove the dependency on the server
- Support using Google Cache for online database

#### 0.5.8
- Stop Etupirka from burning the CPU
- Fix online game data parsing

#### 0.5.7
- 不具合修正: クラッシュを修正

#### 0.5.6
- 不具合修正: ソフト電池DRMのサポートを修正
- 機能追加: 編集距離でゲーム名を予測する機能を追加

## 使い方

### エントリーを追加する
- まずゲームを起動します
- ファイル→プロセスから追加

![](http://i.imgur.com/sSZoDEm.png)

- ゲームプロセスを選択する

![](http://i.imgur.com/LCg9JbT.png)

- エントリーを選択して→設定→ゲーム設定

![](http://i.imgur.com/lWmqFa0.png)

- ErogameScapeのゲームIDを入力してゲーム情報を自動更新（例えば [http://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=4529](http://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=4529) の場合、ゲームIDは4529）


![](http://i.imgur.com/EgwoVnz.png) ![](http://i.imgur.com/uz8PaPR.png)

- これで自動的にゲーム状態を監視するようになります。**ウィンドウがアクティブではない時はカウントしません**


![](http://i.imgur.com/8UQCxx9.png)

### プレイタイム統計を見る
週間や月別などのプレイ時間やゲームごとのプレイ時間を確認することもできます。


![](http://i.imgur.com/XqKcN4n.png) ![](http://i.imgur.com/1A5EK0D.png)


ダブルクリックすると詳しい情報が表示されます。


![](http://i.imgur.com/GQlg57S.png)


### オフラインデータベースを更新する
デフォルトではエロスケをキャッシュしたオフラインデータベースから情報を取得するため、**最新のエロスケ情報を取得するには手動でオフラインデータベースを更新してください**（おそらく月に一回で十分）




## 動作環境
**Windows Vista/7/8/10**（XPは対象外）

**.NET Framework 4.6が必要です**

## Licence
This program is under the MIT license.

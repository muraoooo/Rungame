# Project Overview
- Game Title: Rungame
- High-Level Concept: 2D 横スクロールのアクションゲームで、プレイヤーが右に移動する際に背景がパララックス（視差効果）で動く。
- Players: シングルプレイヤー
- Target Platform: WebGL
- Render Pipeline: UniversalRP (URP)

# Game Mechanics
## Core Gameplay Loop
プレイヤーは左右に移動し、障害物を避けたり攻撃したりしながら進む。背景の雲や山、そして今回追加する林が動くことで、移動のスピード感と奥行きを演出する。

## Controls and Input Methods
- A/D キー または 矢印キー: 左右移動
- スペースキー: ジャンプ (既存)

# UI
(既存のUIを維持)

# Key Asset & Context
- **林のスプライト**: `Assets/Art/Environment/Forest_Layer.png` を生成。既存のプレイヤーや山のスタイルに合わせた、太い黒の輪郭線と緑のグラデーションを持つカートゥーンスタイル。
- **既存のスクリプト**: `Assets/Scripts/Environment/CloudParallaxLoop.cs`
- **修正後のスクリプト**: `EnvironmentParallaxLoop.cs` (汎用化し、ランダムな間隔とサイズに対応)

# Implementation Steps
## Step 1: 林のアセット生成 (Asset Generation)
1. `generate-asset` ツールを使用して、既存のアートスタイルに合致する「林（Forest/Tree line）」のスプライトを生成する。
2. 必要に応じて背景削除を行う。
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: 雲・林のランダム制御スクリプトの作成
1. `CloudParallaxLoop.cs` をベースに、`EnvironmentParallaxLoop.cs` を新規作成（または既存を更新）。
2. `minSpacing`, `maxSpacing` プロパティを追加し、生成時に各要素の間隔をランダムに決定して保持するロジックを実装する。
3. `scaleRange` に基づくサイズランダム化が各要素に確実に適用されるようにする。
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 3: シーンへの林レイヤーの追加
1. シーン内に `ForestParallax` ゲームオブジェクトを作成する。
2. `EnvironmentParallaxLoop` スクリプトをアタッチする。
3. パララックス強度（`parallaxStrength`）を雲（0.12）よりも高い値（例: 0.4）に設定し、山の前、プレイヤーの後ろ（Sorting Order: -5 程度）に配置する。
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: No

## Step 4: 既存の雲のランダム化適用
1. 既存の `FarCloudParallax` オブジェクトのスクリプトを `EnvironmentParallaxLoop` に更新し、ランダム間隔の設定を行う。
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

# Verification & Testing
1. **雲のランダム性確認**: 雲の間隔と大きさが一定ではなく、不規則に並んでいることを確認する。
2. **林の表示と移動**: 林が山（一番奥）とプレイヤーの間に表示され、雲よりも速くプレイヤーと逆方向に動くことを確認する。
3. **ループの整合性**: プレイヤーが移動し続けても、雲と林が途切れることなくループして表示されることを確認する。

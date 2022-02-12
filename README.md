# X1Fold_LaptopSwitcher

Lenovo ThinkPad X1 Foldのラップトップモード切り替えを行います。  

X1 Foldは、キーボードを外すとタブレットモード・キーボードを乗せるとラップトップモードに切り替わります。  
この切り替えは、`LenovoModeSwitcher.exe`によって行われます。  
ModeSwitcherは不安定なソフトなので、ラップトップモード切り替えに特化したソフトを作ることにしました。

# 動作要件
このソフトの動作には、`ModeLib.dll`が必要です。  
`C:\Program Files\Lenovo\Mode Switcher\ModeLib.dll`を本ソフトの実行フォルダ（`X1Fold_LaptopSwitcher.exe`と同じフォルダ）にコピーしてください。

# 実行

`X1Fold_LaptopSwitcher.exe -a`
キーボードの有無を監視し、ラップトップモード/タブレットモードの切り替えを自動で行う。

`X1Fold_LaptopSwitcher.exe -l`
ラップトップモードに切り替える。

`X1Fold_LaptopSwitcher.exe -v`
タブレットモードに切り替える。

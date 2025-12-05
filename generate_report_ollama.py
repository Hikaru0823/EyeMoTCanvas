import subprocess
import datetime
import os


# どのモデルを使うか（インストール済みのものに合わせて変更可）
OLLAMA_MODEL = "llama3"  # 例: "llama3:8b", "phi3", etc.


def get_git_diff() -> str:
    """
    Git の差分を取得する。
    origin/main..HEAD の差分を見ていますが、
    自分の運用に合わせて変えてもOKです。
    """
    try:
        # ここは運用に合わせて変更してOK
        # 例: 前回コミットとの比較 → ["git", "diff", "HEAD~1", "--stat", "--unified=0"]
        result = subprocess.run(
            ["git", "diff", "origin/main..HEAD", "--stat", "--unified=0"],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="ignore",
        )
        return result.stdout
    except FileNotFoundError:
        print("git コマンドが見つかりません。Git がインストールされているか確認してください。")
        return ""


def call_ollama(diff_text: str) -> str:
    """
    ollama をサブプロセスとして呼び出し、日報文を生成してもらう。
    """
    if not diff_text.strip():
        return "本日はコードの変更はありませんでした。"

    # LLM に渡すプロンプト
    prompt = f"""
あなたは Unity ゲーム開発者の日報を作成するアシスタントです。
以下の Git の差分情報から、今日行った作業内容を日本語で要約してください。

出力形式のルール:
- 見出し「本日の作業内容」「明日のTODO」を含める
- 箇条書き（・）で書く
- Unity や C# の開発っぽい表現を意識する
- 難しい専門用語は使いすぎない

【Git diff】
{diff_text}
"""

    try:
        # ollama run MODEL に対して prompt を標準入力で渡す
        result = subprocess.run(
            ["ollama", "run", OLLAMA_MODEL],
            input=prompt,
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="ignore",
        )
    except FileNotFoundError:
        return "ollama コマンドが見つかりません。インストールされているか確認してください。"

    if result.returncode != 0:
        # 何かエラーがあった場合
        return f"ollama 実行時にエラーが発生しました:\n{result.stderr}"

    return result.stdout


def save_report(text: str) -> str:
    """
    日付付きの Markdown ファイルとして保存。
    """
    today = datetime.date.today().strftime("%Y-%m-%d")
    filename = f"daily_report_{today}.md"

    with open(filename, "w", encoding="utf-8") as f:
        f.write(f"# {today} 作業日報\n\n")
        f.write(text.strip())
        f.write("\n")

    return filename


def main():
    diff = get_git_diff()
    if not diff.strip():
        print("差分がありません。日報は生成しません。")
        return

    print("Git の差分を取得しました。ollama による要約を実行します...")
    report_text = call_ollama(diff)
    filename = save_report(report_text)
    print(f"日報を生成しました: {filename}")


if __name__ == "__main__":
    main()

using System;

public interface IQuestManager
{
    int GetQuestCount();

    string GetQuestName(int index);

    bool IsQuestCompleted(int index);

    string GetProgress();

    event Action<int> OnQuestCompleted;
}
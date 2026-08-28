// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Newtonsoft.Json;

namespace SonnyBIM
{
    public class AutoJoinRules : ViewModelBaseNew
    {
        private string _priorityCategory;
        private string _joinWithCategory;
        private bool _isReverse;

        [Obfuscation]
        public string PriorityCategory {
            get => _priorityCategory;
            set
            {
                _priorityCategory = value;
                ConvertPriorityCategoryId();
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public string JoinWithCategory {
            get => _joinWithCategory;
            set
            {
                _joinWithCategory = value;
                ConvertJoinWithCategoryId();
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public bool IsReverse {
            get => _isReverse;
            set
            {
                _isReverse = value;
                OnPropertyChanged();
            }
        }

        internal int PriorityCategoryId { get; set; }
        internal int JoinWithCategoryId { get; set; }



        [JsonConstructor]
        public AutoJoinRules(){}

        public AutoJoinRules(string x = null)
        {
            PriorityCategory = AllJoinCategory.AllCategoryPriority[0];
            JoinWithCategory = AllJoinCategory.AllCutCategory[1];
        }

        public AutoJoinRules(AutoJoinRules rule)
        {
            PriorityCategory = rule.PriorityCategory;
            JoinWithCategory = rule.JoinWithCategory;
            IsReverse = rule.IsReverse;
        }

        public AutoJoinRules(string priorityCategory, string joinWithCategory)
        {
            PriorityCategory = priorityCategory;
            JoinWithCategory = joinWithCategory;
        }

        private void ConvertPriorityCategoryId()
        {
            switch (PriorityCategory)
            {
                case AllJoinCategory.Beam:
                    PriorityCategoryId = AllJoinCategory.BeamCategoryId;
                    break;
            }
        }

        private void ConvertJoinWithCategoryId()
        {
            switch (JoinWithCategory)
            {
                case AllJoinCategory.StructuralColumn:
                    JoinWithCategoryId = AllJoinCategory.StructuralColumnCategoryId;
                    break;
            }
        }
    }
}

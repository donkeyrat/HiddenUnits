using UnityEngine;
using System.Collections.Generic;

namespace HiddenUnits {

    public class HUUnlockConditionsChecker : MonoBehaviour {

        public void Start() { conditions = HUMain.hiddenUnits.LoadAsset<SecretUnlockConditions>("HUUnlockConditions"); }

        public void Update() {

            foreach (var conditioning in conditions.m_unlockConditions) {

                if (!ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret(conditioning.m_unlock)) {

                    List<string> strings = new List<string>();
                    foreach (var condition in conditioning.m_conditionUnlocks) {

                        if (ServiceLocator.GetService<ISaveLoaderService>().HasUnlockedSecret(condition)) { strings.Add(condition); }
                    }
                    conditions.CheckUnlockCondition(strings);
                }
            }
        }


        public SecretUnlockConditions conditions;
    }
}

//Techjuego
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

extern "C"
{
    void PerformUIImpactFeedbackStyleHeavy()
    {
       UIImpactFeedbackGenerator *feedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
       [feedbackGenerator impactOccurred];
    }
    void PerformUIImpactFeedbackStyleMedium()
    {
        UIImpactFeedbackGenerator *feedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        [feedbackGenerator impactOccurred];
    }
    void PerformUIImpactFeedbackStyleLight()
    {
        UIImpactFeedbackGenerator *feedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        [feedbackGenerator impactOccurred];
    }

    void PerformUIImpactFeedbackStyleRigid()
    {
        UIImpactFeedbackGenerator *feedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleRigid];
        [feedbackGenerator impactOccurred];
    }

    void PerformUIImpactFeedbackStyleSoft()
    {
        UIImpactFeedbackGenerator *feedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
        [feedbackGenerator impactOccurred];
    }

    void PerformUINotificationFeedbackTypeSuccess()
    {
        UINotificationFeedbackGenerator *feedbackGenerator = [[UINotificationFeedbackGenerator alloc] init];
        [feedbackGenerator prepare];
        [feedbackGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
    }
    void PerformUINotificationFeedbackTypeError()
    {
        UINotificationFeedbackGenerator *feedbackGenerator = [[UINotificationFeedbackGenerator alloc] init];
        [feedbackGenerator prepare];
        [feedbackGenerator notificationOccurred:UINotificationFeedbackTypeError];
    }
    void PerformUINotificationFeedbackTypeWarning()
    {
        UINotificationFeedbackGenerator *feedbackGenerator = [[UINotificationFeedbackGenerator alloc] init];
        [feedbackGenerator prepare];
        [feedbackGenerator notificationOccurred:UINotificationFeedbackTypeWarning];
    }
    void PerformUISelectionFeedbackGenerator()
    {
        UISelectionFeedbackGenerator *feedbackGenerator = [[UISelectionFeedbackGenerator alloc] init];
        [feedbackGenerator selectionChanged];
    }        
    
}

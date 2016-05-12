using System;

namespace IA.NetBrew.hardware
{
    public class PID
    {

 
        //scaled, tweaked parameters we'll actually be using     
        double kc;                          // * (P)roportional Tuning Parameter     
        double taur;                        // * (I)ntegral Tuning Parameter     
        double taud;                        // * (D)erivative Tuning Parameter     

        //nice, pretty parameters we'll give back to the user if they ask what the tunings are  
        double P_Param;
        double I_Param;
        double D_Param;
        public double myInput { get; set; }


        // * Pointers to the Input, Output, and Setpoint variables     
        public double myOutput { get; set; }            //   This creates a hard link between the variables and the      
        public double mySetpoint { get; set; }         //   PID, freeing the user from having to constantly tell us
              


        double myBias;              // * Pointer to the External FeedForward bias, only used
        //   if the advanced constructor is used     

        bool UsingFeedForward;      // * internal flag that tells us if we're using FeedForward or not      

        ulong nextCompTime;         // * Helps us figure out when the PID Calculation needs to
        //   be performed next                                  
        //   to determine when to compute next     

        ulong tSample;              // * the frequency, in milliseconds, with which we want the
        //   the PID calculation to occur.     

        bool inAuto;                // * Flag letting us know if we are in Automatic or not      

        double lastOutput;          // * remembering the last output is used to prevent
        //   reset windup.     

        double lastInput;           // * we need to remember the last Input Value so we can compute
        //   the derivative required for the D term     

        double accError;            // * the (I)ntegral term is based on the sum of error over
        //   time.  this variable keeps track of that     

        double bias;                // * the base output from which the PID operates                

        double inMin, inSpan;       // * input and output limits, and spans.  used convert     

        double outMin, outSpan;     //   real world numbers into percent span, with which
        //   the PID algorithm is more comfortable.      

        bool justCalced;            // * flag gets set for one cycle after the pid calculates



        /* Standard Constructor (...)***********************************************  
        *   constructor used by most users.  the parameters specified are those for  
        *   for which we can't set up reliable defaults, so we need to have the user  
        *   set them.  
        * ***************************************************************************/
        public PID(double Input, double Output, double Setpoint, double Kc, double TauI, double TauD)
        {
            ConstructorCommon(Input, Output, Setpoint, Kc, TauI, TauD);
            UsingFeedForward = false;
            Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Output"></param>
        /// <param name="Setpoint"></param>
        /// <param name="Kc"></param>
        /// <param name="TauI"></param>
        /// <param name="TauD"></param>
        private void ConstructorCommon(double Input, double Output, double Setpoint, double Kc, double TauI, double TauD)
        {
            SetOutputLimits(0, 100);
            SetInputLimits(0, 1023);

            tSample = 2000;
            SetTunings(Kc, TauI, TauD);

            nextCompTime = (ulong)DateTime.Now.Ticks / 10000;
            inAuto = false;
            myOutput = Output;
            myInput = Input;
            mySetpoint = Setpoint;

        }


        /* SetInputLimits(...)*****************************************************       
         * I don't see this function being called all that much (other than from the  
         * constructor.)  it needs to be here so we can tell the controller what it's   
         * input limits are, and in most cases the 0-1023 default should be fine.  if    
         * there's an application where the signal being fed to the controller is    
         * outside that range, well, then this function's here for you.  
         * **************************************************************************/
        public void SetInputLimits(double INMin, double INMax)
        {
            //After Verifying that mins are smaller than maxes set the value
            if (INMin >= INMax) return;
            inSpan = INMax - INMin;
            inMin = INMin;

            //rescale the working variables to reflect the changes
            lastInput = (lastInput) * (INMax - INMin) / inSpan;
            accError *= (INMax - INMin) / inSpan;

            //Make Sure the working variables are within new limits
            if (lastInput > 1) lastInput = 1;
            else if (lastInput < 0) lastInput = 0;



        }

        /* SetOutputLimits(...)****************************************************  
     * *     This function will be used far more often than SetInputLimits.  while  
     * *  the input to the controller will generally be in the 0-1023 range (which is  
     * *  the default already,)  the output will be a little different.  maybe they'll  
     * *  be doing a time window and will need 0-8000 or something.  or maybe they'll  
     * *  want to clamp it from 0-125.  who knows.  at any rate, that can all be done  
     * *  here. 
     * **************************************************************************/
        public void SetOutputLimits(double OUTMin, double OUTMax)
        {
            //After Verifying that mins are smaller than maxes set the values
            if (OUTMin >= OUTMax) return;
            outMin = OUTMin;
            outSpan = OUTMax - OUTMin;


            //rescale the working variables to reflect the changes
            lastOutput = (lastOutput) * (OUTMax - OUTMin) / (outSpan);

            //Make Sure the working variables are within the new limits
            if (lastOutput > 1) lastOutput = 1;
            else if (lastOutput < 0) lastOutput = 0;

        }

        /* SetTunings(...)*************************************************************  
     * * This function allows the controller's dynamic performance to be adjusted.   
     * * it's called automatically from the constructor, but tunings can also  
     * * be adjusted on the fly during normal operation 
     * ******************************************************************************/
        public void SetTunings(double Kc, double TauI, double TauD)
        {
            //Verify tunings make sense
            if (Kc == 0.0 || TauI < 0.0 || TauD < 0.0) return;
            P_Param = Kc;
            I_Param = TauI;
            D_Param = TauD;
            //convert Reset Time into Reset Rate, and compensate for Calculation frequency         
            var tSampleInSec = (tSample / 1000.0);
            double tempTauR;
            if (TauI == 0.0)
                tempTauR = 0.0;
            else
                tempTauR = (1.0 / TauI) * tSampleInSec;
            if (inAuto)
            {
                //if we're in auto, and we just change the tunings, the integral term                  
                //will become very, very, confused (trust me.) to achieve "bumpless                 
                // transfer" we need to rescale the accumulated error.                 
                if (tempTauR != 0.0) //(avoid divide by 0)                         
                    accError *= (kc * taur) / (Kc * tempTauR);
                else
                    accError = 0.0;
            }
            kc = Kc;
            taur = tempTauR;
            taud = TauD / tSampleInSec;
        }

        /* Reset()*********************************************************************  
        *   does all the things that need to happen to ensure a bumpless transfer  
        *   from manual to automatic mode.  this shouldn't have to be called from the  
        *   outside. In practice though, it is sometimes helpful to start from scratch,  
        *   so it was made publicly available  
        *******************************************************************************/
        public void Reset()
        {
            if (UsingFeedForward)
                bias = (myBias - outMin) / outSpan;
            else bias = (myOutput - outMin) / outSpan;
            lastOutput = bias;
            lastInput = (myInput - inMin) / inSpan;

            // - clear any error in the integral         
            accError = 0;
        }

        /* SetMode(...)****************************************************************  
        *  Allows the controller Mode to be set to manual (0) or Automatic (non-zero)  
        *  when the transition from manual to auto occurs, the controller is  
        *  automatically initialized  
        *******************************************************************************/
        public void SetMode(int Mode)
        {
            if (Mode != 0 && !inAuto)
            {
                //we were in manual, and we just got set to auto.                 
                //reset the controller internals                 
                Reset();
            }
            inAuto = (Mode != 0);
        }

        /* SetSampleTime(...)*******************************************************  
     * * sets the frequency, in Milliseconds, with which the PID calculation is performed       
     * ******************************************************************************/
        public void SetSampleTime(Int32 NewSampleTime)
        {
            if (NewSampleTime <= 0) return;
            //Conver the time-based tunings to reflect this change
            taur *= NewSampleTime / ((double)tSample);
            accError *= tSample / ((double)NewSampleTime);
            taud *= NewSampleTime / ((double)tSample);
            tSample = (ulong)NewSampleTime;
        }

        /* Compute() **********************************************************************  
     * *     This, as they say, is where the magic happens.  this function should be called  
     * *   every time "void loop()" executes.  the function will decide for itself whether a new  
     * *   pid Output needs to be computed  *  
     * *  Some notes for people familiar with the nuts and bolts of PID control:  
     * *  - I used the Ideal form of the PID equation.  mainly because I like IMC  
     * *    tunings.  lock in the I and D, and then just vary P to get more   
     * *    aggressive or conservative  
     * *  
     * *  - While this controller presented to the outside world as being a Reset Time  
     * *    controller, when the user enters their tunings the I term is converted to  
     * *    Reset Rate.  I did this merely to avoid the div0 error when the user wants  
     * *    to turn Integral action off.  
     * *      
     * *  - Derivative on Measurement is being used instead of Derivative on Error.  The  
     * *    performance is identical, with one notable exception.  DonE causes a kick in  
     * *    the controller output whenever there's a setpoint change. DonM does not.  
     * *  
     * *  If none of the above made sense to you, and you would like it to, go to:  
     * *  http://www.controlguru.com .  Dr. Cooper was my controls professor, and is  
     * *  gifted at concisely and clearly explaining PID control  
     * *********************************************************************************/
        public void Compute()
        {
            justCalced = false;
            if (!inAuto) return; //if we're in manual just leave;          
            var now = (ulong)DateTime.Now.Ticks / 10000;
            //millis() wraps around to 0 at some point.  depending on the version of the
            //Arduino Program you are using, it could be in 9 hours or 50 days.
            //this is not currently addressed by this algorithm.     

            //...Perform PID Computations if it's time...         
            if (now < nextCompTime) return;
            //pull in the input and setpoint, and scale them into percent span                 
            var scaledInput = (myInput - inMin) / inSpan;
            if (scaledInput > 1.0) scaledInput = 1.0;
            else if (scaledInput < 0.0) scaledInput = 0.0;

            var scaledSP = (mySetpoint - inMin) / inSpan;
            if (scaledSP > 1.0) scaledSP = 1;
            else if (scaledSP < 0.0) scaledSP = 0;

            //compute the error                 
            var err = scaledSP - scaledInput;

            // check and see if the output is pegged at a limit and only                  
            // integrate if it is not. (this is to prevent reset-windup)                 
            if (!(lastOutput >= 1 && err > 0) && !(lastOutput <= 0 && err < 0))
            {
                accError = accError + err;
            }
            // compute the current slope of the input signal                 
            var dMeas = (scaledInput - lastInput); // we'll assume that dTime (the denominator) is 1 second.
            // if it isn't, the taud term will have been adjusted
            // in "SetTunings" to compensate 

            //if we're using an external bias (i.e. the user used the                  
            //overloaded constructor,) then pull that in now                 
            if (UsingFeedForward)
            {
                bias = (myBias - outMin) / outSpan;
            }

            // perform the PID calculation.                   
            var output = bias + kc * (err + taur * accError - taud * dMeas);

            //make sure the computed output is within output constraints                 
            if (output < 0.0) output = 0.0;
            else if (output > 1.0) output = 1.0;
            lastOutput = output; // remember this output for the windup
            // check next time                               

            lastInput = scaledInput; // remember the Input for the derivative
            // calculation next time

            //scale the output from percent span back out to a real world number                 
            myOutput = ((output * outSpan) + outMin);
            nextCompTime += tSample; // determine the next time the computation
            if (nextCompTime < now) nextCompTime = now + tSample; // should be performed

            justCalced = true; //set the flag that will tell the outside world that the output was just computed 
        }
    }
}

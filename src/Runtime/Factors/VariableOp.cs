// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.ML.Probabilistic.Factors
{
    using System;
    using Microsoft.ML.Probabilistic.Distributions;
    using Microsoft.ML.Probabilistic.Math;
    using Microsoft.ML.Probabilistic.Factors.Attributes;

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Variable<>", Default = true)]
    [FactorMethod(typeof(Factor), "VariableInit<>", Default = true)]
    [Quality(QualityBand.Preview)]
    public static class VariableOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/message_doc[@name="LogEvidenceRatio{T}(T)"]/*'/>
        [Skip]
        public static double LogEvidenceRatio<T>(T use)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/message_doc[@name="MarginalAverageConditional{T}(T, T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [SkipIfAllUniform]
        [MultiplyAll]
        public static T MarginalAverageConditional<T>([NoInit] T use, T def, T result)
            where T : SettableToProduct<T>, SettableTo<T>
        {
            result.SetToProduct(def, use);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/message_doc[@name="MarginalAverageConditionalInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T MarginalAverageConditionalInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/message_doc[@name="UseAverageConditional{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T UseAverageConditional<T>([IsReturned] T Def)
        {
            return Def;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableOp"]/message_doc[@name="DefAverageConditional{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T DefAverageConditional<T>([IsReturned] T use)
        {
            return use;
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableGibbsOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "VariableGibbs<>")]
    [Quality(QualityBand.Preview)]
    public static class VariableGibbsOp
    {
        /// <summary>
        /// Gibbs evidence
        /// </summary>
        /// <returns></returns>
        public static double GibbsEvidence<TDist, T>(TDist Use, TDist Def, GibbsMarginal<TDist, T> marginal)
            where TDist : IDistribution<T>, Sampleable<T>, CanGetLogAverageOf<TDist>
        {
            return Def.GetLogAverageOf(Use) - Def.GetLogProb(marginal.LastSample) - Use.GetLogProb(marginal.LastSample);
        }

        /// <summary>
        /// Gibbs message to 'Marginal'.
        /// </summary>
        /// <param name="Use">Incoming message from 'use'.</param>
        /// <param name="Def">Incoming message from 'def'. Must be a proper distribution.  If uniform, the result will be uniform.</param>
        /// <param name="to_marginal"></param>
        /// <remarks><para>
        /// The outgoing message is the product of 'Def' and 'Uses' messages.
        /// </para></remarks>
        /// <exception cref="ImproperMessageException"><paramref name="Def"/> is not a proper distribution</exception>
        [Stochastic]
        public static GibbsMarginal<TDist, T> MarginalGibbs<TDist, T>(
            TDist Use,
            [SkipIfUniform] TDist Def,
            GibbsMarginal<TDist, T> to_marginal) // must not be called 'result', because its value is used
            where TDist : IDistribution<T>, SettableToProduct<TDist>, SettableTo<TDist>, Sampleable<T>
        {
            GibbsMarginal<TDist, T> result = to_marginal;
            TDist marginal = result.LastConditional;
            marginal.SetToProduct(Def, Use);
            result.LastConditional = marginal;
            // Allow a sample to be drawn from the last conditional, and add it to the sample
            // list and conditional list
            result.PostUpdate();
            return result;
        }

        [Stochastic]
        public static GibbsMarginal<TDist, T> MarginalGibbs<TDist, T>(
            T Use,
            [SkipIfUniform] TDist Def,
            GibbsMarginal<TDist, T> to_marginal) // must not be called 'result', because its value is used
            where TDist : IDistribution<T>, Sampleable<T>
        {
            GibbsMarginal<TDist, T> result = to_marginal;
            TDist marginal = result.LastConditional;
            marginal.Point = Use;
            result.LastConditional = marginal;
            // Allow a sample to be drawn from the last conditional, and add it to the sample
            // list and conditional list
            result.PostUpdate();
            return result;
        }

        /// <summary>
        /// Gibbs sample message to 'Use'
        /// </summary>
        /// <param name="marginal">Incoming message from 'marginal'. Must be a proper distribution.  If uniform, the result will be uniform.</param>
        /// <param name="result">Modified to contain the outgoing message</param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks><para>
        /// The outgoing message is the current Gibbs sample.
        /// </para></remarks>
        /// <exception cref="ImproperMessageException"><paramref name="marginal"/> is not a proper distribution</exception>
        public static T UseGibbs<TDist, T>([SkipIfUniform] GibbsMarginal<TDist, T> marginal, T result)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            return marginal.LastSample;
        }

        /// <summary>
        /// Gibbs distribution message to 'Def'
        /// </summary>
        /// <param name="Def">Incoming message from 'def'. Must be a proper distribution.  If uniform, the result will be uniform.</param>
        /// <param name="result">Modified to contain the outgoing message</param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks><para>
        /// The outgoing message is the product of the 'Def' message with all 'Uses' messages except the current
        /// </para></remarks>
        /// <exception cref="ImproperMessageException"><paramref name="Def"/> is not a proper distribution</exception>
        public static TDist UseGibbs<TDist, T>([IsReturned] TDist Def, TDist result)
            where TDist : SettableTo<TDist>
        {
            result.SetTo(Def);
            return result;
        }

        /// <summary>
        /// Gibbs sample message to 'Def'
        /// </summary>
        /// <param name="marginal">Incoming message from 'marginal'. Must be a proper distribution.  If uniform, the result will be uniform.</param>
        /// <param name="result">Modified to contain the outgoing message</param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks><para>
        /// The outgoing message is the current Gibbs sample.
        /// </para></remarks>
        /// <exception cref="ImproperMessageException"><paramref name="marginal"/> is not a proper distribution</exception>
        public static T DefGibbs<TDist, T>([SkipIfUniform] GibbsMarginal<TDist, T> marginal, T result)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            return marginal.LastSample;
        }

        public static TDist DefGibbs<TDist, T>([IsReturned] TDist Use, TDist result)
            where TDist : IDistribution<T>, SettableTo<TDist>
        {
            result.SetTo(Use);
            return result;
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "VariableMax<>")]
    [Quality(QualityBand.Preview)]
    public static class VariableMaxOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/message_doc[@name="UseMaxConditional{T}(T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T UseMaxConditional<T>(T Def, T result)
            where T : SettableTo<T>
        {
            result.SetTo(Def);
            if (result is UnnormalizedDiscrete)
                ((UnnormalizedDiscrete)(object)result).SetMaxToZero();
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/message_doc[@name="UseMaxConditionalInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T UseMaxConditionalInit<T>([IgnoreDependency] T Def)
            where T : ICloneable
        {
            return (T)Def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/message_doc[@name="DefMaxConditional{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T DefMaxConditional<T>([IsReturned] T Use)
        {
            return Use;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/message_doc[@name="MarginalMaxConditional{T}(T, T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [MultiplyAll]
        public static T MarginalMaxConditional<T>([NoInit] T Use, T Def, T result)
            where T : SettableToProduct<T>, SettableTo<T>
        {
            T res = VariableOp.MarginalAverageConditional<T>(Use, Def, result);
            if (res is UnnormalizedDiscrete)
                ((UnnormalizedDiscrete)(object)res).SetMaxToZero();
            return res;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableMaxOp"]/message_doc[@name="MarginalMaxConditionalInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T MarginalMaxConditionalInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }
    }

#if true
    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Variable<>", Default = true)]
    [FactorMethod(typeof(Factor), "VariableInit<>", Default = true)]
    [Quality(QualityBand.Preview)]
    public static class VariableVmpOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="AverageLogFactor{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static double AverageLogFactor<T>([SkipIfUniform] T to_marginal /*, [IgnoreDependency] T def*/)
            where T : CanGetAverageLog<T>
        {
            return -to_marginal.GetAverageLog(to_marginal);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="MarginalAverageLogarithm{T}(T, T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [SkipIfAllUniform]
        [MultiplyAll]
        public static T MarginalAverageLogarithm<T>([NoInit] T use, T def, T result)
            where T : SettableToProduct<T>, SettableTo<T>
        {
            result.SetToProduct(def, use);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="MarginalAverageLogarithmInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T MarginalAverageLogarithmInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="UseAverageLogarithm{T}(T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T UseAverageLogarithm<T>([IsReturned] T to_marginal, T result)
            where T : SettableTo<T>
        {
            result.SetTo(to_marginal);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="UseAverageLogarithmInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T UseAverageLogarithmInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="VariableVmpOp"]/message_doc[@name="DefAverageLogarithm{T}(T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T DefAverageLogarithm<T>([IsReturned] T to_marginal, T result)
            where T : SettableTo<T>
        {
            result.SetTo(to_marginal);
            return result;
        }
    }
#else
    /// <summary>
    /// Provides outgoing VMP messages for <see cref="Factor.Variable&lt;T&gt;"/>, given random arguments to the function.
    /// </summary>
    [FactorMethod(typeof(Factor), "Variable<>")]
    [FactorMethod(typeof(Factor), "VariableInit<>", Default=false)]
    [Buffers("marginalB")]
    [Quality(QualityBand.Preview)]
    public static class VariableVmpBufferOp
    {
        /// <summary>
        /// Evidence message for VMP
        /// </summary>
        /// <param name="marginal">Outgoing message to 'marginal'.</param>
        /// <returns>Average of the factor's log-value across the given argument distributions</returns>
        /// <remarks><para>
        /// The formula for the result is <c>log(factor(Uses,Def,Marginal))</c>.
        /// Adding up these values across all factors and variables gives the log-evidence estimate for VMP.
        /// </para></remarks>
        public static double AverageLogFactor<T>([Fresh, SkipIfUniform] T marginalB, [IgnoreDependency] T def)
            where T : CanGetAverageLog<T>
        {
            return -marginalB.GetAverageLog(marginalB);
        }

        [SkipIfAllUniform]
        [MultiplyAll]
        public static T MarginalB<T>(T use, T def, T result)
                where T : SettableToProduct<T>, SettableTo<T>
        {
            result.SetToProduct(def, use);
            return result;
        }
        //[Skip]
        //public static T MarginalBInit<T>([IgnoreDependency] T def)
        //  where T : ICloneable
        //{
        //  return (T)def.Clone();
        //}

        public static T MarginalAverageLogarithm<T>([IsReturned] T marginalB, T result)
            where T : SettableTo<T>
        {
            result.SetTo(marginalB);
            return result;
        }

        /// <summary>
        /// VMP message to 'use'
        /// </summary>
        /// <param name="marginal">Current 'marginal'.</param>
        /// <param name="result">Modified to contain the outgoing message</param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks><para>
        /// The outgoing message is the factor viewed as a function of 'use' conditioned on the given values.
        /// </para></remarks>
        public static T UseAverageLogarithm<T>([IsReturned] T marginalB, T result)
            where T : SettableTo<T>
        {
            result.SetTo(marginalB);
            return result;
        }

        /// <summary>
        /// VMP message to 'def'
        /// </summary>
        /// <param name="marginal">Current 'marginal'.</param>
        /// <param name="result">Modified to contain the outgoing message</param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks><para>
        /// The outgoing message is the factor viewed as a function of 'def' conditioned on the given values.
        /// </para></remarks>
        public static T DefAverageLogarithm<T>([IsReturned] T marginalB, T result)
            where T : SettableTo<T>
        {
            result.SetTo(marginalB);
            return result;
        }
    }
    /// <summary>
    /// Provides outgoing VMP messages for <see cref="Factor.Variable&lt;T&gt;"/>, given random arguments to the function.
    /// </summary>
    [FactorMethod(typeof(Factor), "Variable<>")]
    [Buffers("marginalB")]
    [Quality(QualityBand.Preview)]
    public static class VariableNoInitVmpBufferOp
    {
        [Skip]
        public static T MarginalBInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }
    }

    /// <summary>
    /// Provides outgoing VMP messages for <see cref="Factor.Variable&lt;T&gt;"/>, given random arguments to the function.
    /// </summary>
    [FactorMethod(typeof(Factor), "VariableInit<>", Default=true)]
    [Buffers("marginalB")]
    [Quality(QualityBand.Preview)]
    public static class VariableInitVmpBufferOp
    {
        // def is included to get its type as a constraint.  not needed if we could bind on return type.
        public static T MarginalBInit<T>([IgnoreDependency] T def, [SkipIfUniform] T init)
            where T : ICloneable
        {
            return (T)init.Clone();
        }
    }
#endif

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "DerivedVariable<>", Default = true)]
    [FactorMethod(typeof(Factor), "DerivedVariableInit<>", Default = true)]
    [Quality(QualityBand.Preview)]
    public static class DerivedVariableOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="LogAverageFactor()"]/*'/>
        [Skip]
        public static double LogAverageFactor()
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="LogEvidenceRatio()"]/*'/>
        [Skip]
        public static double LogEvidenceRatio<T>(T use)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="MarginalAverageConditional{T}(T, T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [SkipIfAllUniform]
        [MultiplyAll]
        public static T MarginalAverageConditional<T>([NoInit] T Use, T Def, T result)
            where T : SettableToProduct<T>
        {
            result.SetToProduct(Def, Use);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="MarginalAverageConditionalInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T MarginalAverageConditionalInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="UseAverageConditional{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T UseAverageConditional<T>([IsReturned] T Def)
        {
            return Def;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableOp"]/message_doc[@name="DefAverageConditional{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T DefAverageConditional<T>([IsReturned] T Use)
        {
            return Use;
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableGibbsOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "DerivedVariableGibbs<>")]
    [FactorMethod(typeof(Factor), "DerivedVariableInitGibbs<>")]
    [Quality(QualityBand.Preview)]
    public static class DerivedVariableGibbsOp
    {
        #region Gibbs messages

        /// <summary>
        /// Evidence message for Gibbs.
        /// </summary>
        [Skip]
        public static double GibbsEvidence()
        {
            return 0.0;
        }

        /// <summary>
        /// Gibbs sample message to 'Uses'
        /// </summary>
        /// <typeparam name="TDist">Gibbs marginal type</typeparam>
        /// <typeparam name="T">Domain type</typeparam>
        /// <param name="marginal">The Gibbs marginal</param>
        /// <param name="def"></param>
        /// <param name="result">Result</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The outgoing message is the current Gibbs sample.
        /// </para></remarks>
        public static T UseGibbs<TDist, T>([SkipIfUniform] GibbsMarginal<TDist, T> marginal, TDist def, T result)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            // This method must depend on Def, even though Def isn't used, in order to get the right triggers
            return marginal.LastSample;
        }

        /// <summary>
        /// Gibbs sample message to 'Uses'
        /// </summary>
        /// <typeparam name="T">Domain type</typeparam>
        /// <param name="def"></param>
        /// <param name="result">Result</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The outgoing message is the current Gibbs sample.
        /// </para></remarks>
        public static T UseGibbs<T>([IsReturned] T def, T result)
        {
            return def;
        }

        /// <summary>
        /// Gibbs distribution message to 'Def'
        /// </summary>
        /// <typeparam name="TDist">Distribution type</typeparam>
        /// <typeparam name="T">Domain type</typeparam>
        /// <param name="Use">Incoming message from 'Use'.</param>
        /// <remarks><para>
        /// The outgoing message is the product of all the 'Use' messages.
        /// </para></remarks>
        /// <exception cref="ImproperMessageException"><paramref name="Use"/> is not a proper distribution</exception>
        public static TDist DefGibbs<TDist, T>([IsReturned] TDist Use)
        {
            return Use;
        }

        public static T DefGibbs<TDist, T>([SkipIfUniform] GibbsMarginal<TDist, T> marginal, T result)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            return marginal.LastSample;
        }

        /// <summary>
        /// Gibbs message to 'Marginal' for distribution Def
        /// </summary>
        /// <param name="Use">Incoming message from 'Use'.</param>
        /// <param name="Def">Incoming message from 'Def'.</param>
        /// <param name="to_marginal">Previous outgoing message to 'marginal'.</param>
        /// <returns><paramref name="to_marginal"/></returns>
        /// <remarks><para>
        /// The outgoing message is the product of 'Def' and 'Use' messages.
        /// </para></remarks>
        [Stochastic]
        [SkipIfAllUniform]
        public static GibbsMarginal<TDist, T> MarginalGibbs<TDist, T>(
            TDist Use,
            [SkipIfUniform] TDist Def,
            GibbsMarginal<TDist, T> to_marginal)
            where TDist : IDistribution<T>, SettableToProduct<TDist>, Sampleable<T>
        {
            GibbsMarginal<TDist, T> result = to_marginal;
            TDist marginal = result.LastConditional;
            marginal.SetToProduct(Def, Use);
            result.LastConditional = marginal;
            // Allow a sample to be drawn from the last conditional, and add it to the sample
            // list and conditional list
            result.PostUpdate();
            return result;
        }

        /// <summary>
        /// Gibbs message to 'Marginal' for sample Def
        /// </summary>
        /// <typeparam name="TDist"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="Def"></param>
        /// <param name="to_marginal">Previous outgoing message to 'marginal'.</param>
        /// <returns><paramref name="to_marginal"/></returns>
        [Stochastic] // must be labelled Stochastic to get correct schedule, even though it isn't Stochastic
        public static GibbsMarginal<TDist, T> MarginalGibbs<TDist, T>(
            T Def,
            GibbsMarginal<TDist, T> to_marginal)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            GibbsMarginal<TDist, T> result = to_marginal;
            TDist marginal = result.LastConditional;
            marginal.Point = Def;
            result.LastConditional = marginal;
            // Allow a sample to be drawn from the last conditional, and add it to the sample
            // list and conditional list
            result.PostUpdate();
            return result;
        }

        /// <summary>
        /// Gibbs message to 'Marginal' for sample Use
        /// </summary>
        /// <typeparam name="TDist"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="Use"></param>
        /// <param name="Def"></param>
        /// <param name="to_marginal">Previous outgoing message to 'marginal'.</param>
        /// <returns><paramref name="to_marginal"/></returns>
        [Stochastic] // must be labelled Stochastic to get correct schedule, even though it isn't Stochastic
        public static GibbsMarginal<TDist, T> MarginalGibbs<TDist, T>(
            T Use, [IgnoreDependency] TDist Def,
            GibbsMarginal<TDist, T> to_marginal)
            where TDist : IDistribution<T>, Sampleable<T>
        {
            GibbsMarginal<TDist, T> result = to_marginal;
            TDist marginal = result.LastConditional;
            marginal.Point = Use;
            result.LastConditional = marginal;
            // Allow a sample to be drawn from the last conditional, and add it to the sample
            // list and conditional list
            result.PostUpdate();
            return result;
        }

        #endregion
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "DerivedVariableVmp<>", Default = true)]
    [FactorMethod(typeof(Factor), "DerivedVariableInitVmp<>", Default = true)]
    [Quality(QualityBand.Preview)]
    public static class DerivedVariableVmpOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="AverageLogFactor()"]/*'/>
        [Skip]
        public static double AverageLogFactor()
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="MarginalAverageLogarithm{T, TDef}(TDef, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        /// <typeparam name="TDef">The type of the incoming message from <c>Def</c>.</typeparam>
        public static T MarginalAverageLogarithm<T, TDef>([IsReturned] TDef Def, T result)
            where T : SettableTo<TDef>
        {
            result.SetTo(Def);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="MarginalAverageLogarithmInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T MarginalAverageLogarithmInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="UseAverageLogarithm{T, TDef}(TDef, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        /// <typeparam name="TDef"></typeparam>
        public static T UseAverageLogarithm<T, TDef>([IsReturned] TDef Def, T result)
            where T : SettableTo<TDef>
        {
            result.SetTo(Def);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="UseAverageLogarithmInit{T}(T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        [Skip]
        public static T UseAverageLogarithmInit<T>([IgnoreDependency] T def)
            where T : ICloneable
        {
            return (T)def.Clone();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="DerivedVariableVmpOp"]/message_doc[@name="DefAverageLogarithm{T}(T, T)"]/*'/>
        /// <typeparam name="T">The type of the marginal of the variable.</typeparam>
        public static T DefAverageLogarithm<T>(
            [IsReturned] T Use, T result) // must have upward Trigger to match the Trigger on UsesEqualDef.UsesAverageLogarithm
            where T : SettableTo<T>
        {
            result.SetTo(Use);
            return result;
        }
    }
}

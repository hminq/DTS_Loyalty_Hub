export function mapVoucherDefinitionOptions(rawOptions, t) {
  const mapCodes = (codes, prefix) => {
    if (!Array.isArray(codes)) return []
    return codes.map((code) => ({
      value: code,
      label: t(`${prefix}.${code}`, { defaultValue: code }),
    }))
  }

  return {
    rewardTypes: mapCodes(rawOptions?.rewardTypes, 'voucherDefinitions.types.reward'),
    validityTypes: mapCodes(rawOptions?.validityTypes, 'voucherDefinitions.types.validity'),
    publishTypes: mapCodes(rawOptions?.publishTypes, 'voucherDefinitions.types.publish'),
    generationTypes: mapCodes(rawOptions?.generationTypes, 'voucherDefinitions.types.generation'),
    constraints: rawOptions?.constraints,
  }
}

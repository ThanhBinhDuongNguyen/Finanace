"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { authService } from "../../../services/finance/auth.service";
import { importService } from "../../../services/finance/import.service";
import { reconciliationService } from "../../../services/finance/reconciliation.service";
import { exceptionService } from "../../../services/finance/exception.service";
import { ledgerService } from "../../../services/finance/ledger.service";
import { classificationService } from "../../../services/finance/classification.service";

export function useFinanceOperations() {
  const cache = useQueryClient();
  const session = useQuery({ queryKey: ["session"], queryFn: authService.me, retry: false });
  const enabled = !!session.data;
  const imports = useQuery({ queryKey: ["finance", "imports"], queryFn: importService.list, enabled });
  const payouts = useQuery({ queryKey: ["finance", "payouts"], queryFn: reconciliationService.listPayouts, enabled });
  const banks = useQuery({ queryKey: ["finance", "banks"], queryFn: reconciliationService.listBankTransactions, enabled });
  const cases = useQuery({ queryKey: ["finance", "cases"], queryFn: reconciliationService.listCases, enabled });
  const exceptions = useQuery({ queryKey: ["finance", "exceptions"], queryFn: exceptionService.list, enabled });
  const journals = useQuery({ queryKey: ["finance", "journals"], queryFn: ledgerService.list, enabled });
  const mappings = useQuery({ queryKey: ["finance", "mappings"], queryFn: classificationService.listMappings, enabled });
  const classified = useQuery({ queryKey: ["finance", "classified"], queryFn: classificationService.listTransactions, enabled });
  const login = useMutation({ mutationFn: authService.login, onSuccess: async (data) => { cache.removeQueries({ queryKey: ["finance"] }); cache.setQueryData(["session"], data); } });
  const logout = useMutation({ mutationFn: authService.logout, onSuccess: () => { cache.clear(); cache.setQueryData(["session"], null); } });
  const action = useMutation({ mutationFn: (run: () => Promise<unknown>) => run(), onSuccess: () => cache.invalidateQueries({ queryKey: ["finance"] }) });
  return { session, imports, payouts, banks, cases, exceptions, journals, mappings, classified, login, logout, action };
}
